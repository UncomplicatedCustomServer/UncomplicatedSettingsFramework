using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

using static UserSettings.ServerSpecific.SSDropdownSetting;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface IDropdown : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public List<string> Contents { get; set; }
        public DropdownEntryType DropdownEntryType { get; set; }
        public int DefaultContentSelected { get; set; }
    }
}
