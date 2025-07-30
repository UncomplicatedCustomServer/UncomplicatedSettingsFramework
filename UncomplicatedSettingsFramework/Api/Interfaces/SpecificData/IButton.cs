using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface IButton : IData
    {
        public uint Id { get; set; }
        public string Label { get; set; }
        public string? HintDescription { get; set; }
        public string ButtonText { get; set; }
    }
}
