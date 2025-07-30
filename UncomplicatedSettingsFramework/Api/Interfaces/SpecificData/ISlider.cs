using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface ISlider : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public bool Integer { get; set; }
        public string? ValueToStringFormat { get; set; }
        public string? FinalDisplayFormat { get; set; }
    }
}
