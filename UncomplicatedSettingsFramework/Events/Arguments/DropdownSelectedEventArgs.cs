using System;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Events.Arguments
{
    public class DropdownSelectedEventArgs : EventArgs
    {
        public DropdownSelectedEventArgs(SSDropdownSetting dropdown, ReferenceHub hub)
        {
            Dropdown = dropdown;
            ReferenceHub = hub;
        }

        public SSDropdownSetting Dropdown { get; }

        public string SelectedContent => Dropdown.SyncSelectionText;

        public ReferenceHub ReferenceHub { get; }
    }
}
