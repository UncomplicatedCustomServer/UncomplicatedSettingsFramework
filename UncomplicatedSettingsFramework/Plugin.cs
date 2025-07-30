using LabApi.Features.Wrappers;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using System;
using System.IO;
using System.Reflection;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Events;
using UncomplicatedSettingsFramework.Integrations;

namespace UncomplicatedSettingsFramework
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedSettingsFramework";

        public override string Description => "Enables server owners to design and manage Server Specific Settings with ease, no coding required.";

        public override string Author => "Mr. Baguetter";

        public override Version RequiredApiVersion => LabApi.Features.LabApiProperties.CurrentVersion;

        public override Version Version { get; } = new(0, 1, 0);

        public Assembly Assembly => Assembly.GetExecutingAssembly();

        public override LoadPriority Priority => LoadPriority.Highest;

        public static Plugin Instance { get; private set; }

        internal HarmonyLib.Harmony _harmony;

        internal FileConfig FileConfig;

        public override void Enable()
        {
            SettingHandler.Register();
            PlayerHandler.Register();
            ServerHandler.Register();
            Instance = this;
            FileConfig = new();
            if (!File.Exists(Path.Combine(ConfigurationLoader.GetConfigPath(Instance, "UncomplicatedSettingsFramework"), "UncomplicatedSettingsFramework", ".nohttp")))

            _harmony = new($"com.ucs.usf_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            FileConfig.LoadSettings(loadExamples: true);
            FileConfig.LoadSettings(Server.Port.ToString());
            FileConfig.LoadAll();
            FileConfig.LoadAll(Server.Port.ToString());
        }

        public override void Disable()
        {
            SettingHandler.Unregister();
            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            _harmony.UnpatchAll();
            _harmony = null;
            Instance = null;
            FileConfig = null;
        }
    }
}
