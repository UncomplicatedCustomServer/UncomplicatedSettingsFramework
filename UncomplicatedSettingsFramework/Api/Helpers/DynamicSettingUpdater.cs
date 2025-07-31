using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Helpers
{
    public class DynamicSettingUpdater
    {
        private CoroutineHandle _updateCoroutine;

        public void Start()
        {
            if (_updateCoroutine.IsRunning)
                Timing.KillCoroutines(_updateCoroutine);

            Timing.CallDelayed(10, () =>
            {
                _updateCoroutine = Timing.RunCoroutine(UpdateDynamicSettings());
            });
        }

        public void Stop() => Timing.KillCoroutines(_updateCoroutine);

        private IEnumerator<float> UpdateDynamicSettings()
        {
            for (; ; )
            {
                float interval = Plugin.Instance.Config.DynamicUpdateInterval;
                yield return Timing.WaitForSeconds(interval);

                if (CustomSetting.List.Count == 0)
                    continue;

                if (Player.ReadyList.Count() == 0)
                    continue;
                
                foreach (ICustomSetting customSetting in CustomSetting.List)
                {
                    var dynamicDropdowns = customSetting.Settings.Where(s => s.SettingType == SettingType.Dropdown && ((IDropdown)s.CustomData).DynamicContent != DynamicContentType.None).Select(s => (IDropdown)s.CustomData);

                    if (!dynamicDropdowns.Any())
                        continue;

                    foreach (Player player in Player.ReadyList)
                    {
                        foreach (IDropdown dropdownData in dynamicDropdowns)
                        {
                            SSDropdownSetting playerSetting = SSSHelper.GetUserSetting<SSDropdownSetting>(player.ReferenceHub, (int)dropdownData.Id);

                            if (playerSetting == null)
                                continue;

                            string[] newOptions = GetNewOptions(dropdownData.DynamicContent);

                            if (!newOptions.SequenceEqual(playerSetting.Options))
                            {
                                SSSHelper.UpdateUserSettingValue(player.ReferenceHub, (int)dropdownData.Id, typeof(SSDropdownSetting), (settingBase) =>
                                {
                                    if (settingBase is SSDropdownSetting dropdownToUpdate)
                                    {
                                        dropdownToUpdate.Options = newOptions;
                                        if (dropdownToUpdate.SyncSelectionIndexRaw >= newOptions.Length)
                                            dropdownToUpdate.SyncSelectionIndexRaw = 0;
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }

        private string[] GetNewOptions(DynamicContentType type)
        {
            switch (type)
            {
                case DynamicContentType.Players:
                    return Player.ReadyList.Select(p => p.Nickname).ToArray();
                case DynamicContentType.Scps:
                    return Player.ReadyList.Where(p => p.Role.GetTeam() == Team.SCPs).Select(p => p.Nickname).ToArray();
                case DynamicContentType.ClassD:
                    return Player.ReadyList.Where(p => p.Role.GetTeam() == Team.ClassD).Select(p => p.Nickname).ToArray();
                case DynamicContentType.Scientists:
                    return Player.ReadyList.Where(p => p.Role.GetTeam() == Team.Scientists).Select(p => p.Nickname).ToArray();
                case DynamicContentType.Guards:
                    return Player.ReadyList.Where(p => p.Role == RoleTypeId.FacilityGuard).Select(p => p.Nickname).ToArray();
                case DynamicContentType.MTF:
                    return Player.ReadyList.Where(p => p.Role.GetTeam() == Team.FoundationForces).Select(p => p.Nickname).ToArray();
                case DynamicContentType.ChaosInsurgency:
                    return Player.ReadyList.Where(p => p.Role.GetTeam() == Team.ChaosInsurgency).Select(p => p.Nickname).ToArray();
                case DynamicContentType.DeadPlayers:
                    return Player.ReadyList.Where(p => !p.IsAlive).Select(p => p.Nickname).ToArray();
                case DynamicContentType.Roles:
                    return System.Enum.GetNames(typeof(RoleTypeId));
                case DynamicContentType.Teams:
                    return System.Enum.GetNames(typeof(Team));
                case DynamicContentType.Items:
                    return System.Enum.GetNames(typeof(ItemType));
                case DynamicContentType.Zones:
                    return System.Enum.GetNames(typeof(FacilityZone));
                case DynamicContentType.Rooms:
                    return Room.List.Select(r => r.GameObject.name.ToString()).ToArray();
                case DynamicContentType.Doors:
                    return Door.List.Select(d => d.DoorName.ToString()).ToArray();
                case DynamicContentType.Primitives:
                    return AdminToy.List.Select(toy => toy.Base.name).ToArray();
                default:
                    return new string[0];
            }
        }
    }
}