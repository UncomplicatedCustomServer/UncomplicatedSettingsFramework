using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Features.Extensions;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Helpers
{
    public static class SSSHelper
    {
        /// <summary>
        /// Adds or replaces a single setting for this user.
        /// </summary>
        public static void AddOrUpdateUserSetting(ReferenceHub user, object settingOrWrapper)
        {
            ServerSpecificSettingBase setting = settingOrWrapper.ToServerSpecific();
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            for (int i = 0; i < userSettings.Count; i++)
            {
                if (userSettings[i].SettingId == setting.SettingId
                    && userSettings[i].GetType() == setting.GetType())
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

        /// <summary>
        /// Send a batch of settings (and optionally removals) for a user.
        /// </summary>
        public static void SendSettingsToUser(ReferenceHub user, object[] settings, object[] settingsToRemove = null)
        {
            ServerSpecificSettingBase[] toAdd = settings.Select(o => o.ToServerSpecific()).ToArray();
            List<ServerSpecificSettingBase> userSettings = ServerSpecificSettingsSync.ReceivedUserSettings.GetOrAddNew(user);

            if (settingsToRemove != null)
            {
                ServerSpecificSettingBase[] toRem = settingsToRemove.Select(o => o.ToServerSpecific()).ToArray();
                RemoveSettingsFromUser(user, toRem);
            }

            foreach (ServerSpecificSettingBase s in toAdd)
            {
                ServerSpecificSettingBase copy = CreateSettingCopy(s);
                AddOrUpdateUserSetting(user, copy);
            }

            ServerSpecificSettingBase[] existing = ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>();
            ServerSpecificSettingBase[] merged = existing.Select(e => userSettings.FirstOrDefault(u => u.SettingId == e.SettingId && u.GetType() == e.GetType()) ?? e).Concat(userSettings.Where(u => !existing.Any(e => e.SettingId == u.SettingId && e.GetType() == u.GetType()))).ToArray();

            ServerSpecificSettingsSync.DefinedSettings = merged;

            SSSEntriesPack pack = new SSSEntriesPack(merged, ServerSpecificSettingsSync.Version);
            user.connectionToClient.Send(pack);
        }

        /// <summary>
        /// Remove a batch of settings from a user.
        /// </summary>
        public static void RemoveSettingsFromUser(ReferenceHub user, object[] settingsToRemove)
        {
            ServerSpecificSettingBase[] toRem = settingsToRemove.Select(o => o.ToServerSpecific()).ToArray();

            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return;

            var keySet = new HashSet<(int, Type)>(toRem.Select(s => (s.SettingId, s.GetType())));

            userSettings.RemoveAll(s => keySet.Contains((s.SettingId, s.GetType())));

            ServerSpecificSettingBase[] existing = ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>();
            ServerSpecificSettingBase[] merged = existing.Where(e => !keySet.Contains((e.SettingId, e.GetType()))).Concat(userSettings).ToArray();

            ServerSpecificSettingsSync.DefinedSettings = merged;

            SSSEntriesPack pack = new SSSEntriesPack(merged, ServerSpecificSettingsSync.Version);
            user.connectionToClient.Send(pack);
        }

        /// <summary>
        /// Clear *all* settings for this user.
        /// </summary>
        public static void ClearAllUserSettings(ReferenceHub user)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return;

            userSettings.Clear();
            ServerSpecificSettingsSync.ReceivedUserSettings[user] = new List<ServerSpecificSettingBase>();

            ServerSpecificSettingBase[] existing = ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>();
            ServerSpecificSettingBase[] merged = existing.Where(e => !userSettings.Any(u => u.SettingId == e.SettingId && u.GetType() == e.GetType())).ToArray();

            ServerSpecificSettingsSync.DefinedSettings = merged;

            SSSEntriesPack pack = new SSSEntriesPack(merged, ServerSpecificSettingsSync.Version);
            user.connectionToClient.Send(pack);
        }

        public static bool HasUserSetting(ReferenceHub user, int settingId, Type settingType)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return false;

            return userSettings.Any(setting => setting.SettingId == settingId && setting.GetType() == settingType);
        }

        public static bool HasUserSetting(ReferenceHub user, int settingId)
        {
            if (!ServerSpecificSettingsSync.ReceivedUserSettings.TryGetValue(user, out List<ServerSpecificSettingBase> userSettings))
                return false;
                
            return userSettings.Any(setting => setting.SettingId == settingId);
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

            LogManager.Debug($"Checking Setting {userSettings.ToString()}");
            ServerSpecificSettingBase setting = userSettings.FirstOrDefault(s => s.SettingId == settingId && s.GetType() == settingType);
            if (setting != null)
            {
                LogManager.Debug($"Updating setting {setting.Label} ({setting.SettingId}) for user {user.nicknameSync.MyNick}");
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
            if (ServerSpecificSettingsSync.TryGetSettingOfUser<T>(user, settingId, out T result))
            {
                return result;
            }

            return null;
        }
    }
}
