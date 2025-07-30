using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Enums;

namespace UncomplicatedSettingsFramework.Api.Features.Yaml
{
    public class YAMLSetting
    {
        public SettingType SettingType { get; set; }
        public List<Dictionary<string, string>> CustomDataList { get; set; } = new();
        public Dictionary<string, List<string>> Actions { get; set; } = new();
    }
}
