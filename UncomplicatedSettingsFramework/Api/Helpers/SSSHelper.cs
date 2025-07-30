using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UserSettings.ServerSpecific;
using LabApi.Features.Wrappers;
using Mirror;
using UncomplicatedSettingsFramework.Api.Features.Extensions;

namespace UncomplicatedSettingsFramework.Api.Helpers
{
    public class SSSHelper
    {
        public static void AddOrUpdateUserSetting(ReferenceHub user, ServerSpecificSettingBase setting)
        {
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            for (int i = 0; i < userSettings.Count; i++)
            {
                if (userSettings[i].SettingId == setting.SettingId && userSettings[i].GetType() == setting.GetType())
                {
                    userSettings[i] = setting;
                    return;
                }
            }

            userSettings.Add(setting);
        }

        public static ServerSpecificSettingBase CreateSettingCopy(ServerSpecificSettingBase original)
        {
            ServerSpecificSettingBase copy = ServerSpecificSettingsSync.CreateInstance(original.GetType());

            copy.SettingId = original.SettingId;
            copy.Label = original.Label;
            copy.HintDescription = original.HintDescription;
            copy.PlayerPrefsKey = original.PlayerPrefsKey;

            switch (original)
            {
                case SSTwoButtonsSetting twoButtons:
                    SSTwoButtonsSetting twoButtonsCopy = copy as SSTwoButtonsSetting;
                    twoButtonsCopy.OptionA = twoButtons.OptionA;
                    twoButtonsCopy.OptionB = twoButtons.OptionB;
                    twoButtonsCopy.DefaultIsB = twoButtons.DefaultIsB;
                    twoButtonsCopy.SyncIsB = twoButtons.SyncIsB;
                    break;

                case SSButton button:
                    SSButton buttonCopy = copy as SSButton;
                    buttonCopy.ButtonText = button.ButtonText;
                    buttonCopy.HoldTimeSeconds = button.HoldTimeSeconds;
                    break;

                case SSKeybindSetting keybind:
                    SSKeybindSetting keybindCopy = copy as SSKeybindSetting;
                    keybindCopy.SuggestedKey = keybind.SuggestedKey;
                    keybindCopy.PreventInteractionOnGUI = keybind.PreventInteractionOnGUI;
                    keybindCopy.AllowSpectatorTrigger = keybind.AllowSpectatorTrigger;
                    keybindCopy.AssignedKeyCode = keybind.AssignedKeyCode;
                    keybindCopy.SyncIsPressed = keybind.SyncIsPressed;
                    break;

                case SSPlaintextSetting plaintext:
                    SSPlaintextSetting plaintextCopy = copy as SSPlaintextSetting;
                    plaintextCopy.Label = plaintext.Label;
                    plaintextCopy.Placeholder = plaintext.Placeholder;
                    plaintextCopy.ContentType = plaintext.ContentType;
                    plaintextCopy.CharacterLimit = plaintext.CharacterLimit;
                    plaintextCopy.SyncInputText = plaintext.SyncInputText;
                    plaintextCopy._characterLimitOriginalCache = plaintext._characterLimitOriginalCache;
                    break;

                case SSTextArea textArea:
                    SSTextArea textAreaCopy = copy as SSTextArea;
                    textAreaCopy.Label = textArea.Label;
                    textAreaCopy.Foldout = textArea.Foldout;
                    textAreaCopy.AlignmentOptions = textArea.AlignmentOptions;
                    break;

                case SSGroupHeader header:
                    SSGroupHeader headerCopy = copy as SSGroupHeader;
                    headerCopy.Label = header.Label;
                    headerCopy.ReducedPadding = header.ReducedPadding;
                    break;

                case SSDropdownSetting dropdown:
                    SSDropdownSetting dropdownCopy = copy as SSDropdownSetting;
                    dropdownCopy.Options = dropdown.Options;
                    dropdownCopy.DefaultOptionIndex = dropdown.DefaultOptionIndex;
                    dropdownCopy.EntryType = dropdown.EntryType;
                    dropdownCopy.SyncSelectionIndexRaw = dropdown.SyncSelectionIndexRaw;
                    break;

                case SSSliderSetting slider:
                    SSSliderSetting sliderCopy = copy as SSSliderSetting;
                    sliderCopy.MinValue = slider.MinValue;
                    sliderCopy.MaxValue = slider.MaxValue;
                    sliderCopy.SyncFloatValue = slider.SyncFloatValue;
                    sliderCopy.DefaultValue = slider.DefaultValue;
                    break;
            }

            copy.ApplyDefaultValues();
            return copy;
        }

        public static void SendSettingsToUser(ReferenceHub user, ServerSpecificSettingBase[] settings, ServerSpecificSettingBase[]? settingsToRemove)
        {
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            if (settingsToRemove != null)
                RemoveSettingsFromUser(user, settingsToRemove);

            foreach (ServerSpecificSettingBase setting in settings)
            {
                ServerSpecificSettingBase settingCopy = CreateSettingCopy(setting);
                AddOrUpdateUserSetting(user, settingCopy);
            }

            ServerSpecificSettingsSync.DefinedSettings = userSettings.ToArray();
            SSSEntriesPack pack = new SSSEntriesPack(userSettings.ToArray(), ServerSpecificSettingsSync.Version);
            user.connectionToClient.Send(pack);
        }

        public static void RemoveSettingsFromUser(ReferenceHub user, ServerSpecificSettingBase[] settingsToRemove)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return;

            HashSet<(int SettingId, Type SettingType)> settingsToRemoveSet = new HashSet<(int, Type)>();
            foreach (ServerSpecificSettingBase setting in settingsToRemove)
            {
                LogManager.Debug($"Removing {setting.Label}, {setting.SettingId}");
                settingsToRemoveSet.Add((setting.SettingId, setting.GetType()));
            }

            userSettings.RemoveAll(existingSetting => settingsToRemoveSet.Contains((existingSetting.SettingId, existingSetting.GetType())));

            ServerSpecificSettingsSync.DefinedSettings = userSettings.ToArray();
            SSSEntriesPack pack = new SSSEntriesPack(userSettings.ToArray(), ServerSpecificSettingsSync.Version);
            user.connectionToClient.Send(pack);
        }

        public static void ClearAllUserSettings(ReferenceHub user)
        {
            if (ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
            {
                userSettings.Clear();
                ServerSpecificSettingsSync.ReceivedUserSettings[user] = new List<ServerSpecificSettingBase>();
                SSSEntriesPack pack = new SSSEntriesPack(new ServerSpecificSettingBase[0], ServerSpecificSettingsSync.Version);
                user.connectionToClient.Send(pack);
            }
        }

        public static bool HasUserSetting(ReferenceHub user, int settingId, Type settingType)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return false;

            return userSettings.Any(setting => setting.SettingId == settingId && setting.GetType() == settingType);
        }

        /// <summary>
        /// Updates a specific setting's value for a <see cref="Player"/> without recreating it
        /// </summary>
        /// <param name="user"></param>
        /// <param name="settingId"></param>
        /// <param name="settingType"></param>
        /// <param name="updateAction"></param>
        public static void UpdateUserSettingValue(ReferenceHub user, int settingId, Type settingType, Action<ServerSpecificSettingBase> updateAction)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return;

            var setting = userSettings.FirstOrDefault(s => s.SettingId == settingId && s.GetType() == settingType);
            if (setting != null)
            {
                updateAction(setting);
                ServerSpecificSettingsSync.DefinedSettings = userSettings.ToArray();
                SSSEntriesPack pack = new SSSEntriesPack(userSettings.ToArray(), ServerSpecificSettingsSync.Version);
                user.connectionToClient.Send(pack);
            }
        }

        /// <summary>
        /// Gets a specific setting from a <see cref="Player"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <param name="settingId"></param>
        /// <returns></returns>
        public static T GetUserSetting<T>(ReferenceHub user, int settingId) where T : ServerSpecificSettingBase
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return null;

            return userSettings.FirstOrDefault(s => s.SettingId == settingId && s is T) as T;
        }
    }
}
