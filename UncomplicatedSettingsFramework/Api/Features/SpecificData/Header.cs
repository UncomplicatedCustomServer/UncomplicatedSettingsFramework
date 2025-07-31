namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class Header : Data, IHeader
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string Label { get; set; } = string.Empty;
        public virtual string? HintDescription { get; set; } = string.Empty;
        public virtual bool ReducedPadding { get; set; } = false;
    }
}
