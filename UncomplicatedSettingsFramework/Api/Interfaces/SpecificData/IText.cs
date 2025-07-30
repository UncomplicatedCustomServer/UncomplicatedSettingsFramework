using TMPro;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;
using static UserSettings.ServerSpecific.SSTextArea;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface IText : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public FoldoutMode Foldout { get; set; }
        public TextAlignmentOptions AlignmentOptions { get; set; }
    }
}
