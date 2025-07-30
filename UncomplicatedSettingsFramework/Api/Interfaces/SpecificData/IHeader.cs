using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface IHeader : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public bool ReducedPadding { get; set; }
    }
}
