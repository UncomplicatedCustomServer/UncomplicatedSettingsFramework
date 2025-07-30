using LabApi.Events;

namespace UncomplicatedSettingsFramework.Events.Arguments
{
    public static class SettingsEvents
    {
        public static event LabEventHandler<ButtonClickedEventArgs> ButtonClicked;

        public static event LabEventHandler<DropdownSelectedEventArgs> DropdownSelected;

        public static event LabEventHandler<KeybindPressedEventArgs> KeybindPressed;

        public static event LabEventHandler<SliderModifiedEventArgs> SliderModified;

        public static event LabEventHandler<TwoButtonsModifiedEventArgs> TwoButtonsModified;

        internal static void OnButtonClicked(ButtonClickedEventArgs ev) => ButtonClicked.InvokeEvent(ev);

        internal static void OnDropdownSelected(DropdownSelectedEventArgs ev) => DropdownSelected.InvokeEvent(ev);

        internal static void OnKeybindPressed(KeybindPressedEventArgs ev) => KeybindPressed.InvokeEvent(ev);

        internal static void OnSliderModified(SliderModifiedEventArgs ev) => SliderModified.InvokeEvent(ev);

        internal static void OnTwoButtonsModified(TwoButtonsModifiedEventArgs ev) => TwoButtonsModified.InvokeEvent(ev);
    }
}
