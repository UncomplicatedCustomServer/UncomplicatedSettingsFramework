using UnityEngine;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class Keybind : Data, IKeybind
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string Label { get; set; } = string.Empty;
        public virtual string? HintDescription { get; set; } = string.Empty;
        public virtual bool PreventInteractionOnGui { get; set; } = false;
        public virtual bool AllowSpectatorTrigger { get; set; } = false;
        public virtual KeyCode SuggestedKey { get; set; } = KeyCode.None;
    }
}
