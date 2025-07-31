using CommandSystem;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Interfaces;

namespace UncomplicatedSettingsFramework.Api.Commands.Admin
{
    internal class List : ISubcommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "Lists every registered Custom Setting";

        public string VisibleArgs { get; } = string.Empty;

        public int RequiredArgsCount { get; } = 0;

        public string RequiredPermission { get; } = "usf.list";

        public string[] Aliases { get; } = ["l"];

        public bool Execute(List<string> args, ICommandSender sender, out string response)
        {
            response = "\nList of every registered Custom Setting:\n";

            foreach (ICustomSetting setting in CustomSetting.List.OrderBy(setting => setting.Id))
                response += $"<size=23><color=#00ff00>✔</color></size> <size=21>[{setting.Id}]</size> - <color=green>{setting.Name}</color></size>\n";

            if (CustomSetting.UnregisteredList.Count > 0)
            {
                response += $"\nList of every unregistered Custom Setting:\n";

                foreach (ICustomSetting setting in CustomSetting.UnregisteredList.OrderBy(setting => setting.Id))
                    response += $"<size=23><color=#ff0000>❌</color></size> <size=21>[{setting.Id}]</size> - <color=red>{setting.Name}</color></size>\n";
            }

            response += $"\n<color=#00ff00>[✔]</color> {CustomSetting.List.Count} Registered CustomSettings.\n";

            if (CustomSetting.UnregisteredList.Count > 0)
                response += $"<color=#ffff00>[⚠]</color> {CustomSetting.UnregisteredList.Count} Unregistered CustomSettings.";

            return true;
        }
    }
}