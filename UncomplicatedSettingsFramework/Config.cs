#if EXILED
using Exiled.API.Interfaces;
#endif
using System.ComponentModel;

namespace UncomplicatedSettingsFramework
{
#if EXILED
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool ShowSilentLogs { get; set; } = false;

        [Description("Time for every update to Dyanmic Dropdowns. Lower values means more lag")]
        public float DynamicUpdateInterval { get; set; } = 5f;

        [Description("If false, the UCS credit tag system will not be activated. Please do not disable it, as many contributors worked on this plugin for free.")]
        public bool EnableCreditTags { get; set; } = true;

        [Description("If filled the update checker will use the provided token. You can get a token from 'https://github.com/settings/tokens'")]
        public string GithubToken { get; set; } = string.Empty;
    }
#elif LABAPI
    public class Config
    {
        public bool Debug { get; set; } = false;
        public bool ShowSilentLogs { get; set; } = false;

        [Description("Time for every update to Dyanmic Dropdowns. Lower values means more lag")]
        public float DynamicUpdateInterval { get; set; } = 5f;

        [Description("If false, the UCS credit tag system will not be activated. Please do not disable it, as many contributors worked on this plugin for free.")]
        public bool EnableCreditTags { get; set; } = true;

        [Description("If filled the update checker will use the provided token. You can get a token from 'https://github.com/settings/tokens'")]
        public string GithubToken { get; set; } = string.Empty;
    }
#endif
}
