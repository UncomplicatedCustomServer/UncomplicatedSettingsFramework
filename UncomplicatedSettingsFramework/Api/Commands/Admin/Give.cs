using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Interfaces;

namespace UncomplicatedSettingsFramework.Api.Commands.Admin
{
    internal class Give : ISubcommand
    {
        public string Name { get; } = "give";
        public string Description { get; } = "Gives the specified custom setting to the specified player";
        public string VisibleArgs { get; } = "<Setting ID>, <PlayerID>";
        public int RequiredArgsCount { get; } = 2;
        public string RequiredPermission { get; } = "usf.give";
        public string[] Aliases { get; } = ["gi", "giv", "summon"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "Usage: get <Setting ID>, <PlayerID>";
                return false;
            }

            if (!ushort.TryParse(arguments[0], out ushort settingId))
            {
                response = $"Invalid Setting ID: '{arguments[0]}'. Please provide a valid number.";
                return false;
            }

            if (!int.TryParse(arguments[1], out int playerId))
            {
                response = $"Invalid Player ID: '{arguments[1]}'. Please provide a valid number.";
                return false;
            }

            if (!Player.TryGet(playerId, out Player player))
            {
                response = $"Invalid Player ID: '{arguments[1]}'. Please provide a valid number.";
                return false;
            }

            if (!Utilities.TryGetCustomSetting(settingId, out ICustomSetting customSetting))
            {
                response = $"Setting ID {settingId} was not found.";
                return false;
            }

            CustomSetting.RegisterCustomSettingsForPlayer(player.ReferenceHub, customSetting);
            response = $"Registered CustomSetting {customSetting.Name} for player {player.Nickname}";
            return true;
        }
    }
}