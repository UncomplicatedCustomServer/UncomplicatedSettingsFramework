using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Features.Extensions;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Features
{
    public class CustomSetting : ICustomSetting
    {
        /// <summary>
        /// Gets a list of every registered <see cref="ICustomSetting"/>
        /// </summary>
        public static List<ICustomSetting> List => CustomSettings.Values.ToList();

        /// <summary>
        /// Gets a list of every unregistered <see cref="ICustomSetting"/>
        /// </summary>
        public static List<ICustomSetting> UnregisteredList => UnregisteredCustomSettings.Values.ToList();

        internal static Dictionary<uint, ICustomSetting> CustomSettings { get; set; } = new();
        internal static Dictionary<uint, ICustomSetting> UnregisteredCustomSettings { get; set; } = new();

        /// <summary>
        /// Register a new <see cref="ICustomSetting"/> inside the plugin
        /// </summary>
        /// <param name="setting"></param>
        public static void Register(ICustomSetting setting)
        {
            if (!Utilities.CustomSettingValidator(setting, out string error))
            {
                LogManager.Warn($"Unable to register the ICustomSetting with the Id {setting.Id} and name '{setting.Name}':\n{error}\nError code: 0x029");
                UnregisteredCustomSettings.TryAdd(setting.Id, setting);
                return;
            }
            CustomSettings.TryAdd(setting.Id, setting);
            LogManager.Info($"Successfully registered ICustomSetting '{setting.Name}' (Id: {setting.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="ICustomSetting"/> from the plugin by its class
        /// </summary>
        /// <param name="setting"></param>
        public static void Unregister(ICustomSetting setting) => Unregister(setting.Id);

        /// <summary>
        /// Unregister a <see cref="ICustomSetting"/> from the plugin by its Id
        /// </summary>
        /// <param name="setting"></param>
        public static void Unregister(uint setting)
        {
            if (CustomSettings.ContainsKey(setting) || UnregisteredCustomSettings.ContainsKey(setting))
            {
                CustomSettings.Remove(setting);
                UnregisteredCustomSettings.Remove(setting);
            }
        }

        public static void RegisterCustomSettingsForPlayer(ReferenceHub user, ICustomSetting customSetting)
        {
            ServerSpecificSettingBase[] settings = PrepareCustomSettings(customSetting);
            if (settings != null)
                SSSHelper.SendSettingsToUser(user, settings, null);
        }

        public static void RegisterCustomSettingsAllPlayers(ICustomSetting customSetting)
        {
            ServerSpecificSettingBase[] settings = PrepareCustomSettings(customSetting);
            if (settings == null) return;
            
            foreach (Player player in Player.ReadyList)
                SSSHelper.SendSettingsToUser(player.ReferenceHub, settings, null);
        }

        private static ServerSpecificSettingBase[] PrepareCustomSettings(ICustomSetting customSetting)
        {   
            if (customSetting.SpawnData.Any(s => s.MinPlayersRequired != null && s.MinPlayersRequired > Player.ReadyList.Count()))
                return null;
            
            return CustomSettingConverter.ToServerSpecificSettings(customSetting);
        }

        /// <summary>
        /// Gets the first free Id for a CustomSetting
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static uint GetFirstFreeId(uint from = 0)
        {
            for (uint i = from; i < uint.MaxValue; i++)
                if (!CustomSettings.ContainsKey(i))
                    return i;

            return 0;
        }

        public virtual uint Id { get; set; }
        public virtual string Name { get; set; }
        public virtual List<Spawn> SpawnData { get; set; }
        public virtual string? RequiredPermission { get; set; }
        public virtual List<Setting> Settings { get; set; }
    }
}
