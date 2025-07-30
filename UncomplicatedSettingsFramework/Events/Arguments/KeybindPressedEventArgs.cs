using System;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Events.Arguments
{
    public class KeybindPressedEventArgs : EventArgs
    {
        public KeybindPressedEventArgs(SSKeybindSetting keybind, ReferenceHub hub)
        {
            Keybind = keybind;
            ReferenceHub = hub;
        }

        public SSKeybindSetting Keybind { get; }

        public ReferenceHub ReferenceHub { get; }
    }
}
