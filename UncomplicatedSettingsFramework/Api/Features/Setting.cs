using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Features
{
    public class Setting : ISetting
    {
        public virtual SettingType SettingType { get; set; } = SettingType.Dropdown;
        public virtual IData CustomData { get; set; } = new Dropdown();
        public virtual Dictionary<string, List<string>> Actions { get; set; } = new();
    }
}
