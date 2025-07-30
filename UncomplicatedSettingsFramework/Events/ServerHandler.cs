using System.Linq;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Interfaces;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;

namespace UncomplicatedSettingsFramework.Events
{
    public class ServerHandler
    {
        public static void Register()
        {
            ServerEvent.WaveRespawned += OnWaveSpawn;
        }
        public static void Unregister()
        {
            ServerEvent.WaveRespawned -= OnWaveSpawn;
        }

        public static void OnWaveSpawn(WaveRespawnedEventArgs ev)
        {
            foreach (ICustomSetting customSetting in CustomSetting.List)
            {
                foreach (ISpawn spawnData in customSetting.SpawnData)
                {
                    if (!spawnData.SendOnTeamSpawn?.Any(teamSpawn => teamSpawn.Faction == ev.Wave.Faction && teamSpawn.Send == true) == true)
                        continue;

                    foreach (Player player in ev.Players)
                    {
                        if (spawnData.AllowedRoles == null ||  spawnData.AllowedRoles.Count == 0 ||  spawnData.AllowedRoles.Contains(player.Role) && string.IsNullOrEmpty(customSetting.RequiredPermission) || player.HasPermissions(customSetting.RequiredPermission))
                            CustomSetting.RegisterCustomSettingsForPlayer(player.ReferenceHub, customSetting);
                    }
                }
            }
        }
    }
}