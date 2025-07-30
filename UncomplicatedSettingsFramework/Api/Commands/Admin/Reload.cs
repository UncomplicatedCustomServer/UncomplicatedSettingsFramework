using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Api.Interfaces;

namespace UncomplicatedSettingsFramework.Api.Commands.Admin
{

    internal class Reload : ISubcommand
    {
        public string Name { get; } = "reload";

        public string Description { get; } = "Reloads all custom settings";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "usf.reload";

        public string[] Aliases { get; } = ["r, re"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count > 0)
            {
                response = "This command doesnt have any arguments";
                return false;
            }

            foreach (Player player in Player.ReadyList)
                SSSHelper.ClearAllUserSettings(player.ReferenceHub);

            foreach (ICustomSetting customSetting in CustomSetting.List.ToList())
            {
                CustomSetting.Unregister(customSetting.Id);
                LogManager.Debug($"Unregistered {customSetting.Name}.");
            }

            FileConfig FileConfig = Plugin.Instance.FileConfig;
            ActiveCustomSetting.List.Clear();
            CustomSetting.List.Clear();
            CustomSetting.UnregisteredList.Clear();

            FileConfig.LoadSettings(loadExamples: true);
            FileConfig.LoadSettings(Server.Port.ToString());
            FileConfig.LoadAll();
            FileConfig.LoadAll(Server.Port.ToString());

            foreach (ICustomSetting customSetting in CustomSetting.List.ToList())
                foreach (Player player in Player.ReadyList)
                {
                    if (!string.IsNullOrEmpty(customSetting.RequiredPermission) && player.HasPermissions(customSetting.RequiredPermission))
                    {
                        CustomSetting.RegisterCustomSettingsForPlayer(player.ReferenceHub, customSetting);
                        LogManager.Debug($"Registered {customSetting.Name} for player {player.Nickname} (Id: {player.PlayerId})");
                    }
                    else
                    {
                        CustomSetting.RegisterCustomSettingsForPlayer(player.ReferenceHub, customSetting);
                        LogManager.Debug($"Registered {customSetting.Name} for player {player.Nickname} (Id: {player.PlayerId})");
                    }
                }

            response = $"\nReloaded {CustomSetting.List.Count} Custom settings. \n Amount of unregistered Custom Settings: {CustomSetting.UnregisteredList.Count}";
            return true;
        }
    }
}
