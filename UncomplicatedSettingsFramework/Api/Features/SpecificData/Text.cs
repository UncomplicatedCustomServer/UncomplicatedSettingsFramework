using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using static UserSettings.ServerSpecific.ServerSpecificSettingBase;
using static UserSettings.ServerSpecific.SSTextArea;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class Text : Data, IText
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string Label { get; set; } = string.Empty;
        public virtual string? HintDescription { get; set; } = string.Empty;
        public virtual FoldoutMode Foldout { get; set; }
        public virtual TextAlignmentOptions AlignmentOptions { get; set; }
    }
}
