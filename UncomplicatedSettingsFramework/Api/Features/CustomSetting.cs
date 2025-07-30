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
        /// <param name="item"></param>
        public static void Register(ICustomSetting item)
        {
            if (!Utilities.CustomSettingValidator(item, out string error))
            {
                LogManager.Warn($"Unable to register the ICustomSetting with the Id {item.Id} and name '{item.Name}':\n{error}\nError code: 0x029");
                UnregisteredCustomSettings.TryAdd(item.Id, item);
                return;
            }
            CustomSettings.TryAdd(item.Id, item);
            LogManager.Info($"Successfully registered ICustomSetting '{item.Name}' (Id: {item.Id}) into the plugin!");
        }

        /// <summary>
        /// Unregister a <see cref="ICustomSetting"/> from the plugin by its class
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(ICustomSetting item) => Unregister(item.Id);

        /// <summary>
        /// Unregister a <see cref="ICustomSetting"/> from the plugin by its Id
        /// </summary>
        /// <param name="item"></param>
        public static void Unregister(uint item)
        {
            if (CustomSettings.ContainsKey(item) || UnregisteredCustomSettings.ContainsKey(item))
            {
                CustomSettings.Remove(item);
                UnregisteredCustomSettings.Remove(item);
            }
        }

        public static void RegisterCustomSettingsForPlayer(ReferenceHub user, ICustomSetting customSetting)
        {
            List<ServerSpecificSettingBase> allServerSettings = new List<ServerSpecificSettingBase>();
            ServerSpecificSettingBase[] serverSettings = CustomSettingConverter.ToServerSpecificSettings(customSetting);
            allServerSettings.AddRange(serverSettings);

            SSSHelper.SendSettingsToUser(user, allServerSettings.ToArray(), null);
        }

        public static void RegisterCustomSettingsAllPlayers(ICustomSetting customSetting)
        {
            List<ServerSpecificSettingBase> allServerSettings = new List<ServerSpecificSettingBase>();
            ServerSpecificSettingBase[] serverSettings = CustomSettingConverter.ToServerSpecificSettings(customSetting);
            allServerSettings.AddRange(serverSettings);

            foreach (Player player in Player.ReadyList)
                SSSHelper.SendSettingsToUser(player.ReferenceHub, allServerSettings.ToArray(), null);
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
