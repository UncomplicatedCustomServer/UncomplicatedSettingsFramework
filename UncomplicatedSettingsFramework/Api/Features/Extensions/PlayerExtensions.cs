using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using PlayerEvent = LabApi.Events.Handlers.PlayerEvents;

namespace UncomplicatedSettingsFramework.Api.Features.Extensions
{
    public static class PlayerExtensions
    {
        public static void Register()
        {
            PlayerEvent.Death += OnPlayerKilled;
            PlayerEvent.Left += OnPlayerLeft;
        }

        public static void Unregister()
        {
            PlayerEvent.Death -= OnPlayerKilled;
            PlayerEvent.Left -= OnPlayerLeft;
        }

        public static Dictionary<int, int> PlayerKills { get; set; } = [];

        public static void OnPlayerKilled(PlayerDeathEventArgs ev)
        {
            if (ev.Attacker is null)
                return;
            
            if (PlayerKills.ContainsKey(ev.Attacker.PlayerId))
                PlayerKills[ev.Attacker.PlayerId]++;
            else
                PlayerKills[ev.Attacker.PlayerId] = 1;
        }

        public static void OnPlayerLeft(PlayerLeftEventArgs ev) => PlayerKills.TryRemove(ev.Player.PlayerId);

        /// <summary>
        /// Gets the number of kills for the specified player
        /// </summary>
        /// <param name="player">The player to get kills for</param>
        /// <returns>Number of kills, or 0 if player has no kills</returns>
        public static int Kills(this Player player) => PlayerKills.TryGetValue(player.PlayerId, out int kills) ? kills : 0;
    }
}