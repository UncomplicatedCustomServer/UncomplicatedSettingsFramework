using TMPro;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class PlainText : Data, IPlainText
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string SyncInputText { get; set; } = string.Empty;
        public virtual string? Placeholder { get; set; } = string.Empty;
        public virtual TMP_InputField.ContentType ContentType { get; set; } = TMP_InputField.ContentType.Standard;
        public virtual int CharacterLimit { get; set; } = 100;
    }
}
