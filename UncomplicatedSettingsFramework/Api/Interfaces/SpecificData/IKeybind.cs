using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;
using UnityEngine;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface IKeybind : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public bool PreventInteractionOnGui { get; set; }
        public bool AllowSpectatorTrigger { get; set; }
        public KeyCode SuggestedKey { get; set; }
    }
}
