using System;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Events.Arguments
{
    public class SliderModifiedEventArgs : EventArgs
    {
        public SliderModifiedEventArgs(SSSliderSetting slider, ReferenceHub hub)
        {
            Slider = slider;
            ReferenceHub = hub;
            Value = slider.SyncFloatValue;
        }

        public SSSliderSetting Slider { get; }

        public float Value { get; }

        public ReferenceHub ReferenceHub { get; }
    }
}
