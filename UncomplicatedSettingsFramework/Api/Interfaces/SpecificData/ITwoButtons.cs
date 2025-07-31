using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface ITwoButtons : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public bool DefaultIsB { get; set; }
    }
}
