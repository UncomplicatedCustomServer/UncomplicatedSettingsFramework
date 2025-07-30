using PlayerRoles;

namespace UncomplicatedSettingsFramework.Api.Features.SpawnData
{
    public class SendOnTeamSpawn
    {
        public Faction Faction { get; set; } = Faction.FoundationStaff;
        public bool Send { get; set; } = true;
    }
}