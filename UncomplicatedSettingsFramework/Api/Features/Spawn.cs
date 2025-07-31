using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using UncomplicatedSettingsFramework.Api.Features.SpawnData;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Features
{
    public class Spawn : ISpawn
    {
        public virtual List<SendOnTeamSpawn>? SendOnTeamSpawn { get; set; } =
        [
            new SendOnTeamSpawn
            {
                Faction = Faction.FoundationEnemy,
                Send = true
            }
        ];
        public virtual bool? SendOnJoin { get; set; } = false;
        public virtual bool? SendToAll { get; set; } = false;
        public virtual bool? RemoveOnDeath { get; set; } = true;
        public virtual int? MinPlayersRequired { get; set; } = 3;
        public virtual List<FacilityZone>? AllowedZones { get; set; } = null;
        public virtual int? RequiredKills { get; set; } = null;
        public virtual List<RoleTypeId> AllowedRoles { get; set; } =
        [
            RoleTypeId.ChaosConscript,
            RoleTypeId.ClassD,
            RoleTypeId.Tutorial,
            RoleTypeId.ChaosMarauder,
            RoleTypeId.ChaosRepressor,
            RoleTypeId.ChaosRifleman
        ];

        private CoroutineHandle _playerZoneCheck;

        public void Start()
        {
            if (_playerZoneCheck.IsRunning)
                Timing.KillCoroutines(_playerZoneCheck);

            Timing.CallDelayed(10, () =>
            {
                _playerZoneCheck = Timing.RunCoroutine(PlayerZoneCheck());
            });
        }

        public void Stop() => Timing.KillCoroutines(_playerZoneCheck);

        private IEnumerator<float> PlayerZoneCheck()
        {
            for (; ; )
            {
                float interval = Plugin.Instance.Config.DynamicUpdateInterval;
                yield return Timing.WaitForSeconds(interval);
                
                if (!Round.IsRoundStarted)
                    continue;

                if (CustomSetting.List.Count == 0)
                    continue;

                if (Player.ReadyList.Count() == 0)
                    continue;

                foreach (ICustomSetting customSetting in CustomSetting.List)
                {
                    foreach (Player player in Player.ReadyList)
                    {
                        if (customSetting.SpawnData.Any(s => s.AllowedZones != null && s.AllowedZones.Contains(player.Room.Zone)) && string.IsNullOrEmpty(customSetting.RequiredPermission) || player.HasPermissions(customSetting.RequiredPermission) && !SSSHelper.HasUserSetting(player.ReferenceHub, (int)customSetting.Id))
                            CustomSetting.RegisterCustomSettingsForPlayer(player.ReferenceHub, customSetting);
                        else
                        {
                            ServerSpecificSettingBase[] serverSettings = CustomSettingConverter.ToServerSpecificSettings(customSetting);
                            SSSHelper.RemoveSettingsFromUser(player.ReferenceHub, serverSettings);
                        }
                    }
                }
            }
        }
    }
}
