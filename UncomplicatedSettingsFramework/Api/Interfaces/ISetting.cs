using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Interfaces
{
    internal interface ISetting
    {
        public SettingType SettingType { get; set; }
        public IData CustomData { get; set; }
        public Dictionary<string, List<string>> Actions { get; set; }
    }
}
