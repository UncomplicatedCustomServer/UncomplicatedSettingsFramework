using System.Collections.Generic;
using MapGeneration;
using PlayerRoles;
using UncomplicatedSettingsFramework.Api.Features.SpawnData;

namespace UncomplicatedSettingsFramework.Api.Interfaces
{
    public interface ISpawn
    {
        public List<SendOnTeamSpawn>? SendOnTeamSpawn { get; set; }
        public bool? SendOnJoin { get; set; }
        public bool? SendToAll { get; set; }
        public bool? RemoveOnDeath { get; set; }
        public int? MinPlayersRequired { get; set; }
        public List<FacilityZone>? AllowedZones { get; set; }
        public int? RequiredKills { get; set; }
        public List<RoleTypeId> AllowedRoles { get; set; }
    }
}
