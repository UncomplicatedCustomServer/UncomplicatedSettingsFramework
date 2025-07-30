using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api
{
    /// <summary>
    /// Handles all the <see cref="Utilities"/> needed for USF.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Check if a <see cref="ICustomSetting"/> is valid and can be registered
        /// </summary>
        /// <param name="item">The custom setting item to validate</param>
        /// <param name="error">Output parameter containing error details if validation fails</param>
        /// <returns><see cref="bool"/> <see langword="true"/> if valid, <see langword="false"/> if there's any problem</returns>
        public static bool CustomSettingValidator(ICustomSetting item, out string error)
        {
            error = string.Empty;

            if (item == null)
            {
                error = "Custom setting item cannot be null.";
                return false;
            }

            if (CustomSetting.CustomSettings.ContainsKey(item.Id))
            {
                uint oldId = item.Id;
                uint newId = CustomSetting.GetFirstFreeId(1);
                item.Id = newId;
                LogManager.Warn($"{item.Name} - ID {oldId} is already in use. Assigning new ID {newId}.");
                CustomSetting.Register(item);
            }

            if (item.Settings == null)
            {
                error = "Settings collection cannot be null.";
                return false;
            }

            foreach (Setting setting in item.Settings)
            {
                if (setting?.CustomData == null)
                {
                    error = "Setting or CustomData cannot be null.";
                    return false;
                }

                if (!IsValidSettingType(setting, out string validationError))
                {
                    error = validationError;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates that a setting's CustomData matches its declared SettingType
        /// </summary>
        /// <param name="setting">The setting to validate</param>
        /// <param name="error">Output parameter containing error details if validation fails</param>
        /// <returns>True if the setting type matches its CustomData interface</returns>
        private static bool IsValidSettingType(ISetting setting, out string error)
        {
            IData customData = setting.CustomData;
            string actualTypeName = customData.GetType().Name;

            var validationRules = new Dictionary<SettingType, (Type expectedInterface, string interfaceName)>
            {
                { SettingType.Button, (typeof(IButton), nameof(IButton)) },
                { SettingType.Dropdown, (typeof(IDropdown), nameof(IDropdown)) },
                { SettingType.Header, (typeof(IHeader), nameof(IHeader)) },
                { SettingType.Keybind, (typeof(IKeybind), nameof(IKeybind)) },
                { SettingType.PlainText, (typeof(IPlainText), nameof(IPlainText)) },
                { SettingType.Slider, (typeof(ISlider), nameof(ISlider)) },
                { SettingType.Text, (typeof(IText), nameof(IText)) },
                { SettingType.TwoButtons, (typeof(ITwoButtons), nameof(ITwoButtons)) }
            };

            if (!validationRules.TryGetValue(setting.SettingType, out var rule))
            {
                error = $"Unknown SettingType '{setting.SettingType}'. Please report this issue on our Discord server: https://discord.gg/5StRGu8EJV";
                return false;
            }

            if (!rule.expectedInterface.IsAssignableFrom(customData.GetType()))
            {
                error = $"Setting type mismatch: '{setting.SettingType}' requires '{rule.interfaceName}' interface, but found '{actualTypeName}'. Please ensure the CustomData implements the correct interface.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Check if a <see cref="ICustomSetting"/> is valid and can be registered.
        /// Does not return the error as text!
        /// </summary>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="false"/> if there's any problem.</returns>
        public static bool CustomSettingValidator(ICustomSetting item)
        {
            return CustomSettingValidator(item, out _);
        }

        /// <summary>
        /// Try to get a <see cref="ICustomSetting"/> by it's <see cref="ICustomSetting.Id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if the item exists and <paramref name="item"/> is not <see langword="null"/> or <see langword="default"/></returns>
        public static bool TryGetCustomSetting(uint id, out ICustomSetting item) => CustomSetting.CustomSettings.TryGetValue(id, out item);

        /// <summary>
        /// Try to get a <see cref="ICustomSetting"/> by it's <see cref="ICustomSetting.Name"/>
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="item"></param>
        /// <returns><see cref="bool"/> <see langword="true"/> if the item exists and <paramref name="item"/> is not <see langword="null"/> or <see langword="default"/></returns>
        public static bool TryGetCustomSettingByName(string Name, out ICustomSetting item)
        {
            item = CustomSetting.List.FirstOrDefault(i => i.Name == Name);
            return item != null;
        }

        /// <summary>
        /// Get a <see cref="ICustomSetting"/> by it's <see cref="ICustomSetting.Id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns><see cref="ICustomSetting"/> if it exists, otherwhise a <see langword="default"/> will be returned</returns>
        public static ICustomSetting GetCustomSetting(uint id) => CustomSetting.CustomSettings[id];

        /// <summary>
        /// Check if the given <see cref="ICustomSetting.Id"/> is already registered as a <see cref="ICustomSetting"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsCustomSetting(uint id) => CustomSetting.CustomSettings.ContainsKey(id);
    }
}
