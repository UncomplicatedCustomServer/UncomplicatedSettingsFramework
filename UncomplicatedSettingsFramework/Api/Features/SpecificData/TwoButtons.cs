using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedSettingsFramework.Api.Features.SpecificData
{
    public class TwoButtons : Data, ITwoButtons
    {
        public virtual uint Id { get; set; } = 4;
        public virtual string Label { get; set; } = string.Empty;
        public virtual string? HintDescription { get; set; } = string.Empty;
        public virtual bool SyncIsB { get; set; }
        public virtual string OptionA { get; set; }
        public virtual string OptionB { get; set; }
        public virtual bool DefaultIsB { get; set; }
    }
}
