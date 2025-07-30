using System.Collections.Generic;
using PlayerRoles;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features.SpawnData;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Interfaces
{
    public interface ISpawn
    {
        public List<SendOnTeamSpawn>? SendOnTeamSpawn { get; set; }
        public bool? SendOnJoin { get; set; }
        public bool? SendToAll { get; set; }
        public bool? RemoveOnDeath { get; set; }
        public List<RoleTypeId> AllowedRoles { get; set; }
    }
}
