using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using PlayerRoles.FirstPersonControl;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UserSettings.ServerSpecific;
using Utils.Networking;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedSettingsFramework.Events
{
    public class PlayerHandler
    {
        public static void Register()
        {
            PlayerEvent.Joined += OnPlayerJoin;
            PlayerEvent.Death += OnDeath;
            PlayerEvent.Spawned += OnSpawned;
        }
        public static void Unregister()
        {
            PlayerEvent.Joined -= OnPlayerJoin;
            PlayerEvent.Death -= OnDeath;
            PlayerEvent.Spawned -= OnSpawned;
        }

        public static void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            foreach (ICustomSetting customSetting in CustomSetting.List)
                foreach (ISpawn spawnData in customSetting.SpawnData)
                {
                    bool hasValidRole = spawnData.AllowedRoles.Contains(ev.Player.Role);
                    bool hasRequiredPermission = string.IsNullOrEmpty(customSetting.RequiredPermission) || ev.Player.HasPermissions(customSetting.RequiredPermission);

                    if (hasValidRole && hasRequiredPermission)
                    {
                        CustomSetting.RegisterCustomSettingsForPlayer(ev.Player.ReferenceHub, customSetting);
                        break;
                    }
                }
        }

        public static void OnPlayerJoin(PlayerJoinedEventArgs ev)
        {
            foreach (Player player in Player.ReadyList)
                new SyncedScaleMessages.ScaleMessage(player.ReferenceHub.transform.localScale, player.ReferenceHub).SendToAuthenticated();

            foreach (ICustomSetting customSetting in CustomSetting.List)
            {
                if (customSetting.SpawnData.Any(x => x.SendToAll == true) || customSetting.SpawnData.Any(x => x.SendOnJoin == true))
                {
                    if (string.IsNullOrEmpty(customSetting.RequiredPermission) || ev.Player.HasPermissions(customSetting.RequiredPermission))
                    {
                        LogManager.Debug($"Sending custom setting '{customSetting.Name}' to player {ev.Player.Nickname} (Id: {ev.Player.PlayerId}) on join.");
                        CustomSetting.RegisterCustomSettingsForPlayer(ev.Player.ReferenceHub, customSetting);
                    }
                }
            }
        }


        public static void OnDeath(PlayerDeathEventArgs ev)
        {
            foreach (ICustomSetting customSetting in CustomSetting.List)
            {
                if (customSetting.SpawnData.Any(s => s.RemoveOnDeath == true) && string.IsNullOrEmpty(customSetting.RequiredPermission) || ev.Player.HasPermissions(customSetting.RequiredPermission))
                {
                    ServerSpecificSettingBase[] serverSettings = CustomSettingConverter.ToServerSpecificSettings(customSetting);
                    SSSHelper.RemoveSettingsFromUser(ev.Player.ReferenceHub, serverSettings);
                }
            }
        }
    }
}
