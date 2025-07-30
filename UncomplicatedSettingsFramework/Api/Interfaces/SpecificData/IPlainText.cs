using TMPro;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public interface IPlainText : IData
    {
        public uint Id { get; set; }
        public string SyncInputText { get; set; }
        public string? Placeholder { get; set; }
        public TMP_InputField.ContentType ContentType { get; set; }
        public int CharacterLimit { get; set; }
    }
}
