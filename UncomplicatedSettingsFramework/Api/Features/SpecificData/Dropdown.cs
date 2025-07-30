using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UserSettings.ServerSpecific.SSDropdownSetting;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class Dropdown : Data, IDropdown
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string Label { get; set; } = string.Empty;
        public virtual string? HintDescription { get; set; } = string.Empty;
        public virtual List<string> Contents { get; set; } = new();
        public virtual DropdownEntryType DropdownEntryType { get; set; } = DropdownEntryType.Regular;
        public virtual int DefaultContentSelected { get; set; } = 0;
    }
}
