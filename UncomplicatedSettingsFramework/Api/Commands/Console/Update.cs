using CommandSystem;
using System.Threading.Tasks;
using System;
using System.IO;
using RemoteAdmin;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using LabApi.Features.Wrappers;

#if EXILED
using Exiled.API.Features;
#endif

namespace UncomplicatedSettingsFramework.Api.Commands.Console
{
    public class GitHubAssetInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Update : ParentCommand
    {
        private const string PluginDllName = "UncomplicatedSettingsFramework.dll";

        public Update() => LoadGeneratedCommands();

        public override string Command { get; } = "usfupdate";
        public override string[] Aliases { get; } = new string[] { "usfselfupdate" };
        public override string Description { get; } = "Downloads and installs the latest version of UncomplicatedSettingsFramework, then restarts the server round.";

        public override void LoadGeneratedCommands() { }

        private string GetPluginPath()
        {
#if EXILED
            return Path.Combine(Paths.Plugins, PluginDllName);
#elif LABAPI
            return Plugin.Instance.FilePath;
#else
            return Plugin.Instance.FilePath;
#endif
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Version version = Plugin.Instance.Version;
            LogManager.Info($"Current UncomplicatedSettingsFramework version: {version}. Attempting to update...");
            response = $"Attempting to update UncomplicatedSettingsFramework from version {version}. Check console for details.";

            Task.Run(async () =>
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "UncomplicatedSettingsFramework-Updater/1.1");

                        if (!string.IsNullOrEmpty(Plugin.Instance.Config.GithubToken))
                            client.DefaultRequestHeaders.Add("Authorization", $"token {Plugin.Instance.Config.GithubToken}");

                        string apiUrl = "https://api.github.com/repos/UncomplicatedCustomServer/UncomplicatedSettingsFramework/releases/latest";
                        HttpResponseMessage httpResponse = await client.GetAsync(apiUrl);

                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            string errorContent = await httpResponse.Content.ReadAsStringAsync();
                            LogManager.Error($"Failed to fetch latest release info from GitHub. Status: {httpResponse.StatusCode}. Response: {errorContent}");
                            return;
                        }

                        string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                        GitHubReleaseInfo latestRelease = JsonConvert.DeserializeObject<GitHubReleaseInfo>(jsonResponse);

                        if (latestRelease == null || latestRelease.Assets == null || !latestRelease.Assets.Any())
                        {
                            LogManager.Error("Failed to parse release information or no assets found in the latest release.");
                            return;
                        }

                        GitHubAssetInfo asset = latestRelease.Assets.FirstOrDefault(asset => asset.Name.Equals(PluginDllName, StringComparison.OrdinalIgnoreCase));

                        if (asset == null || string.IsNullOrEmpty(asset.BrowserDownloadUrl))
                        {
                            LogManager.Error($"Could not find the plugin DLL ('{PluginDllName}') in the latest GitHub release assets, or download URL is missing.");
                            return;
                        }

                        string latestVersionTag = latestRelease.TagName;
                        if (latestVersionTag.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                            latestVersionTag = latestVersionTag.Substring(1);

                        if (Version.TryParse(latestVersionTag, out Version latestGitHubVersion))
                        {
                            LogManager.Updater($"Latest version on GitHub: {latestGitHubVersion} (Tag: {latestRelease.TagName}). Current version: {version}.");
                            if (latestGitHubVersion <= version && arguments.FirstOrDefault()?.ToLower() != "force")
                            {
                                LogManager.Updater($"You are already running version {version} or newer. To force update, use 'usfupdate force'.");
                                return;
                            }
                        }
                        else
                            LogManager.Warn($"Could not parse latest GitHub version tag '{latestRelease.TagName}'. Proceeding with download if forced or newer by asset name.");

                        LogManager.Updater($"Downloading {PluginDllName} from {asset.BrowserDownloadUrl}...");
                        byte[] fileBytes = await client.GetByteArrayAsync(asset.BrowserDownloadUrl);

                        if (fileBytes == null || fileBytes.Length == 0)
                        {
                            LogManager.Error("Downloaded file is empty.");
                            return;
                        }

                        string pluginPath = GetPluginPath();
                        if (string.IsNullOrEmpty(pluginPath))
                        {
                            LogManager.Error("Could not determine the path of the current plugin DLL. Update aborted.");
                            return;
                        }

                        LogManager.Updater($"Current plugin location: {pluginPath}");
                        LogManager.Updater("Attempting to overwrite plugin DLL. The server will attempt to restart the round after this.");

                        try
                        {
                            File.WriteAllBytes(pluginPath, fileBytes);
                            LogManager.Updater($"{PluginDllName} downloaded and replaced successfully ({fileBytes.Length} bytes).");
                            Server.RunCommand("rnr");
                        }
                        catch (IOException ex)
                        {
                            LogManager.Error($"IO Error writing plugin file: {ex.Message}. Ensure the server has write permissions and the file is not locked. A manual restart might be required after placing the DLL.");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            LogManager.Error($"Unauthorized Access Error writing plugin file: {ex.Message}. Ensure the server process has write permissions to the plugins directory.");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error($"Error saving plugin DLL or executing rnr: {ex.ToString()}");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogManager.Error($"A network error occurred during update: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    LogManager.Error($"Error parsing JSON response from GitHub: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"An unexpected error occurred during update: {ex.ToString()}");
                }
            });
            return true;
        }
    }
}