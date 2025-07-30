using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Helpers
{
    internal class CustomSettingConverter
    {
        public static ServerSpecificSettingBase[] ToServerSpecificSettings(ICustomSetting customSetting)
        {
            List<ServerSpecificSettingBase> serverSettings = new List<ServerSpecificSettingBase>();

            foreach (var setting in customSetting.Settings)
            {
                ServerSpecificSettingBase serverSetting = null;

                switch (setting.SettingType)
                {
                    case SettingType.Button:
                        var buttonData = (IButton)setting.CustomData;
                        serverSetting = new SSButton(
                            (int?)buttonData.Id,
                            buttonData.Label,
                            buttonData.HintDescription,
                            null,
                            buttonData.ButtonText
                        );
                        break;

                    case SettingType.Dropdown:
                        var dropdownData = (IDropdown)setting.CustomData;
                        serverSetting = new SSDropdownSetting(
                            (int)dropdownData.Id,
                            dropdownData.Label,
                            dropdownData.Contents.ToArray(),
                            dropdownData.DefaultContentSelected,
                            dropdownData.DropdownEntryType,
                            dropdownData.HintDescription
                        );
                        break;

                    case SettingType.Header:
                        var headerData = (IHeader)setting.CustomData;
                        serverSetting = new SSGroupHeader(
                            headerData.Label,
                            headerData.ReducedPadding,
                            headerData.HintDescription
                        )
                        {
                            SettingId = (int)customSetting.Id
                        };
                        break;

                    case SettingType.Keybind:
                        var keybindData = (IKeybind)setting.CustomData;
                        serverSetting = new SSKeybindSetting(
                            (int)keybindData.Id,
                            keybindData.Label,
                            keybindData.SuggestedKey,
                            keybindData.PreventInteractionOnGui,
                            keybindData.AllowSpectatorTrigger,
                            keybindData.HintDescription
                        );
                        break;

                    case SettingType.PlainText:
                        var plainTextData = (IPlainText)setting.CustomData;
                        serverSetting = new SSPlaintextSetting(
                            (int)plainTextData.Id,
                            plainTextData.Placeholder,
                            plainTextData.SyncInputText,
                            plainTextData.CharacterLimit,
                            plainTextData.ContentType
                        );
                        break;

                    case SettingType.Slider:
                        var sliderData = (ISlider)setting.CustomData;
                        serverSetting = new SSSliderSetting(
                            (int)sliderData.Id,
                            sliderData.Label,
                            sliderData.MinValue,
                            sliderData.MaxValue,
                            sliderData.DefaultValue,
                            sliderData.Integer,
                            sliderData.HintDescription,
                            sliderData.ValueToStringFormat,
                            sliderData.FinalDisplayFormat
                        );
                        break;

                    case SettingType.Text:
                        var textData = (IText)setting.CustomData;
                        serverSetting = new SSTextArea(
                            (int)textData.Id,
                            textData.Label,
                            textData.Foldout,
                            textData.HintDescription,
                            textData.AlignmentOptions
                        );
                        break;

                    case SettingType.TwoButtons:
                        var twoButtonsData = (ITwoButtons)setting.CustomData;
                        serverSetting = new SSTwoButtonsSetting(
                            (int)twoButtonsData.Id,
                            twoButtonsData.Label,
                            twoButtonsData.HintDescription,
                            twoButtonsData.OptionA,
                            twoButtonsData.DefaultIsB,
                            twoButtonsData.OptionB
                        );
                        break;
                }

                if (serverSetting != null)
                {
                    serverSettings.Add(serverSetting);
                }
            }

            return serverSettings.ToArray();
        }
    }
}
