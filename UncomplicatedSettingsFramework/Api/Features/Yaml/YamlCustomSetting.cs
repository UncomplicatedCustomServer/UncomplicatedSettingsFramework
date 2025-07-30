using System.Collections.Generic;

namespace UncomplicatedSettingsFramework.Api.Features.Yaml
{
    /// <summary>
    /// The YAML refrence for CustomSettings.
    /// </summary>
    public class YAMLCustomSetting
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public List<Spawn> SpawnData { get; set; }
        public string? RequiredPermission { get; set; }
        public List<YAMLSetting> Settings { get; set; }
    }
}
