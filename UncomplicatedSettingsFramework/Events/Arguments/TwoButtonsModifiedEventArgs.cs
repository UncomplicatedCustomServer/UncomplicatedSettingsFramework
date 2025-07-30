using System;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Events.Arguments
{
    public class TwoButtonsModifiedEventArgs : EventArgs
    {
        public TwoButtonsModifiedEventArgs(SSTwoButtonsSetting twobuttons, ReferenceHub hub)
        {
            TwoButtons = twobuttons;
            ReferenceHub = hub;
        }

        public SSTwoButtonsSetting TwoButtons { get; }

        public ReferenceHub ReferenceHub { get; }
    }
}
