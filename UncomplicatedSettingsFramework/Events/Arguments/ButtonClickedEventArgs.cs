using System;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Events.Arguments
{
    public class ButtonClickedEventArgs : EventArgs
    {
        public ButtonClickedEventArgs(SSButton button, ReferenceHub hub)
        {
            Button = button;
            ReferenceHub = hub;
        }

        public SSButton Button { get; }

        public ReferenceHub ReferenceHub { get; }
    }
}
