using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Events.Arguments;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Events
{
    public class SettingHandler
    {
        public static void Register()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += SSSSEvents;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnValueReceived;
        }
        public static void Unregister()
        {
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= SSSSEvents;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= OnValueReceived;
        }

        private static readonly Dictionary<Type, string> _settingActionMap = new()
        {
            { typeof(SSButton), "OnClicked" },
            { typeof(SSDropdownSetting), "OnSelected" },
            { typeof(SSKeybindSetting), "OnPressed" },
            { typeof(SSSliderSetting), "OnModified" },
            { typeof(SSTextArea), "OnSubmitted" },
        };

        public static void OnValueReceived(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is null)
            {
                LogManager.Error("Received null settingBase in OnValueReceived.");
                return;
            }

            if (!Player.TryGet(referenceHub.gameObject, out Player player))
                return;

            Setting foundSetting = CustomSetting.List.SelectMany(customSetting => customSetting.Settings).FirstOrDefault(s =>
            {
                PropertyInfo idProperty = s.CustomData?.GetType().GetProperty("Id");
                if (idProperty == null)
                    return false;

                object idValue = idProperty.GetValue(s.CustomData);
                return idValue is uint id && id == (uint)settingBase.SettingId;
            });

            if (foundSetting is null)
                return;

            if (settingBase is SSDropdownSetting dropdown)
            {
                if (foundSetting.Actions.TryGetValue("OnSelected", out List<string> actions))
                {
                    List<string> processedActions = new List<string>();
                    string dynamicPlaceholder = $"{{dropdown.{dropdown.SettingId}.selection}}";
                    string genericPlaceholder = "{dropdown.selection}";

                    foreach (string action in actions)
                    {
                        string processedAction = action.Replace(dynamicPlaceholder, dropdown.SyncSelectionText).Replace(genericPlaceholder, dropdown.SyncSelectionText);
                        processedActions.Add(processedAction);
                    }
                    ActionManager.ExecuteActions(processedActions, player);
                }
            }

            else if (settingBase is SSTwoButtonsSetting twoButtons)
            {
                List<string> actionsToExecute = new List<string>();

                if (twoButtons.SyncIsA && foundSetting.Actions.TryGetValue("OnPressedA", out List<string> actionsA))
                    actionsToExecute.AddRange(actionsA);

                if (twoButtons.SyncIsB && foundSetting.Actions.TryGetValue("OnPressedB", out List<string> actionsB))
                    actionsToExecute.AddRange(actionsB);

                if (actionsToExecute.Any())
                    ActionManager.ExecuteActions(actionsToExecute, player);
            }
            else if (_settingActionMap.TryGetValue(settingBase.GetType(), out string actionName) && foundSetting.Actions.TryGetValue(actionName, out List<string> actions))
            {
                ActionManager.ExecuteActions(actions, player);
            }
        }

        public static void SSSSEvents(ReferenceHub referenceHub, ServerSpecificSettingBase settingBase)
        {
            if (settingBase is null)
            {
                LogManager.Error("Received null settingBase in OnValueReceived.");
                return;
            }

            switch (settingBase)
            {
                case SSButton button:
                    {
                        ButtonClickedEventArgs eventArgs = new ButtonClickedEventArgs(button, referenceHub);
                        SettingsEvents.OnButtonClicked(eventArgs);
                        break;
                    }
                case SSDropdownSetting dropdown:
                    {
                        DropdownSelectedEventArgs eventArgs = new DropdownSelectedEventArgs(dropdown, referenceHub);
                        SettingsEvents.OnDropdownSelected(eventArgs);
                        break;
                    }
                case SSKeybindSetting keybind:
                    {
                        KeybindPressedEventArgs eventArgs = new KeybindPressedEventArgs(keybind, referenceHub);
                        SettingsEvents.OnKeybindPressed(eventArgs);
                        break;
                    }
                case SSSliderSetting slider:
                    {
                        SliderModifiedEventArgs eventArgs = new SliderModifiedEventArgs(slider, referenceHub);
                        SettingsEvents.OnSliderModified(eventArgs);
                        break;
                    }
                case SSTwoButtonsSetting twobuttons:
                    {
                        TwoButtonsModifiedEventArgs eventArgs = new TwoButtonsModifiedEventArgs(twobuttons, referenceHub);
                        SettingsEvents.OnTwoButtonsModified(eventArgs);
                        break;
                    }
            }
        }
    }
}
