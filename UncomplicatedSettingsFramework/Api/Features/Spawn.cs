using System.Collections.Generic;
using PlayerRoles;
using UncomplicatedSettingsFramework.Api.Features.SpawnData;
using UncomplicatedSettingsFramework.Api.Interfaces;

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
        public virtual List<RoleTypeId> AllowedRoles { get; set; } =
        [
            RoleTypeId.ChaosConscript,
            RoleTypeId.ClassD,
            RoleTypeId.Tutorial,
            RoleTypeId.ChaosMarauder,
            RoleTypeId.ChaosRepressor,
            RoleTypeId.ChaosRifleman
        ];
    }
}
