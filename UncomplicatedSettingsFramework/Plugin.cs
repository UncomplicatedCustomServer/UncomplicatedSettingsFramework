#if LABAPI
using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Loader;
using LabApi.Events.Handlers;
#elif EXILED
using Exiled.API.Enums;
using Exiled.API.Features;
#endif
using System;
using System.IO;
using System.Reflection;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.Extensions;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Helper;
using UncomplicatedSettingsFramework.Api.Helpers;
using UncomplicatedSettingsFramework.Events;
using ServerEvent = LabApi.Events.Handlers.ServerEvents;
using HarmonyLib;

namespace UncomplicatedSettingsFramework
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedSettingsFramework";
#if EXILED
        public override string Prefix => "UncomplicatedSettingsFramework";
#elif LABAPI
        public override string Description => "Enables server owners to design and manage Server Specific Settings with ease, no coding required.";
#endif
        public override string Author => "Mr. Baguetter";
#if EXILED
        public override Version RequiredExiledVersion { get; } = new(9, 6, 1);
#elif LABAPI
        public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;
#endif
        public override Version Version { get; } = new(0, 1, 0);

        public Assembly Assembly => Assembly.GetExecutingAssembly();
#if EXILED
        public override PluginPriority Priority => PluginPriority.First;
#elif LABAPI
        public override LoadPriority Priority => LoadPriority.Highest;
#endif
        public static Plugin Instance { get; private set; }

        internal Harmony _harmony;

        internal DynamicSettingUpdater DynamicUpdater;

        internal Spawn spawn;

        internal static HttpManager HttpManager;

        internal FileConfig FileConfig;
#if LABAPI
        public override void Enable()
        {
            Instance = this;

            ServerEvent.WaitingForPlayers += OnWaitingForPlayers;
            HttpManager = new("usf");
            DynamicUpdater = new();
            DynamicUpdater.Start();
            PlayerExtensions.Register();
            SettingHandler.Register();
            spawn = new();
            spawn.Start();
            PlayerHandler.Register();
            ServerHandler.Register();
            PlayerExtensions.PlayerKills.Clear();
            HttpManager.RegisterEvents();
            FileConfig = new();
            if (!File.Exists(Path.Combine(ConfigurationLoader.GetConfigPath(Instance, "UncomplicatedSettingsFramework"), "UncomplicatedSettingsFramework", ".nohttp")))
#if LABAPI
                _harmony = new($"com.ucs.usf_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#elif EXILED
            _harmony = new($"com.ucs.usf_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
#endif
            _harmony.PatchAll();

            LogManager.History.Clear();
            FileConfig.LoadSettings(loadExamples: true);
            FileConfig.LoadSettings(Server.Port.ToString());
            FileConfig.LoadAll();
            FileConfig.LoadAll(Server.Port.ToString());
        }

        public override void Disable()
        {
            ServerEvent.WaitingForPlayers -= OnWaitingForPlayers;
            DynamicUpdater?.Stop();
            PlayerExtensions.Unregister();
            SettingHandler.Unregister();
            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            HttpManager.UnregisterEvents();
            spawn.Stop();
            _harmony.UnpatchAll();
            _harmony = null;
            Instance = null;
            spawn = null;
            FileConfig = null;
            DynamicUpdater = null;
        }
#elif EXILED
        public override void OnEnabled()
        {
            Instance = this;

            ServerEvent.WaitingForPlayers += OnWaitingForPlayers;
            HttpManager = new("usf");
            DynamicUpdater = new();
            DynamicUpdater.Start();
            PlayerExtensions.Register();
            SettingHandler.Register();
            spawn = new();
            spawn.Start();
            PlayerHandler.Register();
            ServerHandler.Register();
            PlayerExtensions.PlayerKills.Clear();
            HttpManager.RegisterEvents();
            FileConfig = new();

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedSettingsFramework", ".nohttp")))

            _harmony = new($"com.ucs.usf_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            LogManager.History.Clear();
            FileConfig.LoadSettings(loadExamples: true);
            FileConfig.LoadSettings(Server.Port.ToString());
            FileConfig.LoadAll();
            FileConfig.LoadAll(Server.Port.ToString());

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            ServerEvent.WaitingForPlayers -= OnWaitingForPlayers;
            DynamicUpdater?.Stop();
            PlayerExtensions.Unregister();
            SettingHandler.Unregister();
            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            HttpManager.UnregisterEvents();
            spawn.Stop();
            _harmony.UnpatchAll();
            _harmony = null;
            Instance = null;
            spawn = null;
            FileConfig = null;
            DynamicUpdater = null;
            base.OnDisabled();

        }
#endif
            private void OnWaitingForPlayers() => _ = Updater.CheckForUpdatesAsync();
    }
}
