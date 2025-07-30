using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Features;

namespace UncomplicatedSettingsFramework.Api.Interfaces
{
    public interface ICustomSetting
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public List<Spawn> SpawnData { get; set; }
        public string? RequiredPermission { get; set; }
        public List<Setting> Settings { get; set; }
    }
}
