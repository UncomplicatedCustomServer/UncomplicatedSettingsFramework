﻿#if EXILED
using Exiled.API.Features;
#endif
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace UncomplicatedSettingsFramework.Api.Features.Helper
{
    public class GitHubReleaseInfo
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("assets")]
        public GitHubAssetInfo[] Assets { get; set; }
    }

    public class GitHubAssetInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }

    /// <summary>
    /// A static helper class to handle checking for and applying updates from GitHub.
    /// </summary>
    public static class Updater
    {
#if EXILED
        private const string PluginDllName = "UncomplicatedSettingsFramework-Exiled.dll";
#elif LABAPI
        private const string PluginDllName = "UncomplicatedSettingsFramework-LabApi.dll";
#endif
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Checks GitHub for a newer version of the plugin.
        /// </summary>
        public static async Task CheckForUpdatesAsync()
        {
            try
            {
                var currentVersion = Plugin.Instance.Version;
                LogManager.Updater($"Current version: {currentVersion}. Checking for updates...");

                var latestRelease = await GetLatestReleaseAsync();
                if (latestRelease == null) return;

                string latestVersionTag = latestRelease.TagName.TrimStart('v');
                if (Version.TryParse(latestVersionTag, out Version githubVersion))
                {
                    LogManager.Updater($"Latest version on GitHub: {githubVersion}.");
                    if (githubVersion > currentVersion)
                    {
                        LogManager.Updater($"An update is available! Use the 'usfupdate' command to install it.");
                    }
                    else
                    {
                        LogManager.Updater("You are on the latest version.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"An unexpected error occurred during update check: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads and installs the latest version of the plugin.
        /// </summary>
        public static async Task UpdatePluginAsync(Version currentVersion, string forceArgument)
        {
            try
            {
                var latestRelease = await GetLatestReleaseAsync();
                if (latestRelease == null) return;

                var asset = latestRelease.Assets?.FirstOrDefault(a => a.Name.Equals(PluginDllName, StringComparison.OrdinalIgnoreCase));
                if (asset == null || string.IsNullOrEmpty(asset.BrowserDownloadUrl))
                {
                    LogManager.Error($"Could not find the plugin DLL ('{PluginDllName}') in the latest GitHub release.");
                    return;
                }

                string latestVersionTag = latestRelease.TagName.TrimStart('v');
                if (Version.TryParse(latestVersionTag, out Version latestGitHubVersion) && latestGitHubVersion <= currentVersion && forceArgument?.ToLower() != "force")
                {
                    LogManager.Updater("You are already on the latest version. Use 'usfupdate force' to proceed anyway.");
                    return;
                }

                LogManager.Updater($"Downloading new version from {asset.BrowserDownloadUrl}...");
                byte[] fileBytes = await HttpClient.GetByteArrayAsync(asset.BrowserDownloadUrl);
                LogManager.Updater($"{PluginDllName} updated successfully ({fileBytes.Length} bytes). Restarting round to apply changes.");

                await Task.Run(() => File.WriteAllBytes(GetPluginPath(), fileBytes));
                LabApi.Features.Wrappers.Server.RunCommand("rnr");
            }
            catch (Exception ex)
            {
                LogManager.Error($"An unexpected error occurred during the update process: {ex}");
            }
        }

        private static async Task<GitHubReleaseInfo> GetLatestReleaseAsync()
        {
            try
            {
                HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UncomplicatedSettingsFramework-Updater/1.2");
                if (!string.IsNullOrEmpty(Plugin.Instance.Config.GithubToken))
                {
                    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", Plugin.Instance.Config.GithubToken);
                }

                string apiUrl = "https://api.github.com/repos/UncomplicatedCustomServer/UncomplicatedSettingsFramework/releases/latest";
                HttpResponseMessage httpResponse = await HttpClient.GetAsync(apiUrl);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    LogManager.Error($"Failed to fetch release info from GitHub. Status: {httpResponse.StatusCode}");
                    return null;
                }

                string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GitHubReleaseInfo>(jsonResponse);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Network error while fetching release info: {ex.Message}");
                return null;
            }
        }

        private static string GetPluginPath()
        {
#if EXILED
            return Path.Combine(Paths.Plugins, PluginDllName);
#else
            return Plugin.Instance.FilePath;
#endif
        }
    }
}