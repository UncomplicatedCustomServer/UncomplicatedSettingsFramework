namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class Slider : Data, ISlider
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string Label { get; set; } = string.Empty;
        public virtual string? HintDescription { get; set; } = string.Empty;
        public virtual float MinValue { get; set; } = 0f;
        public virtual float MaxValue { get; set; } = 1f;
        public virtual float DefaultValue { get; set; } = 0.5f;
        public virtual bool Integer { get; set; }
        public virtual string? ValueToStringFormat { get; set; }
        public virtual string? FinalDisplayFormat { get; set; }

    }
}
