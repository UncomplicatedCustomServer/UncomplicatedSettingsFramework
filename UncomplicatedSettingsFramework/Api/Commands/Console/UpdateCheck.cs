using CommandSystem;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UncomplicatedSettingsFramework.Api.Features.Helper;

namespace UncomplicatedSettingsFramework.Api.Commands.Console
{
    public class GitHubReleaseInfo
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("assets")]
        public GitHubAssetInfo[] Assets { get; set; }
    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class UpdateCheck : ParentCommand
    {
        public UpdateCheck() => LoadGeneratedCommands();

        public override string Command { get; } = "usfupdatecheck";
        public override string[] Aliases { get; } = new string[] { "usfcheckupdate" };
        public override string Description { get; } = "Checks if a new version of UncomplicatedSettingsFramework is available.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            Version version = Plugin.Instance.Version;
            response = $"Currently running version {version}. Checking for updates...";
            
            Task.Run(async () => await UpdateChecker.CheckForUpdatesAsync());
            return true;
        }
    }
}
