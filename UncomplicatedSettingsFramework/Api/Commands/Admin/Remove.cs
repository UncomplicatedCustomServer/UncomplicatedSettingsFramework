using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Api.Interfaces;

namespace UncomplicatedSettingsFramework.Api.Commands.Admin
{
    internal class Remove : ISubcommand
    {
        public string Name { get; } = "remove";
        public string Description { get; } = "Removes the specified custom setting to the specified player";
        public string VisibleArgs { get; } = "<Setting ID>, <PlayerID>";
        public int RequiredArgsCount { get; } = 2;
        public string RequiredPermission { get; } = "usf.remove";
        public string[] Aliases { get; } = ["r", "re", "take"];

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

            SSSHelper.RemoveSettingsFromUser(player.ReferenceHub, CustomSettingConverter.ToServerSpecificSettings(customSetting));
            response = $"Removed CustomSetting {customSetting.Name} for player {player.Nickname}";
            return true;
        }
    }
}