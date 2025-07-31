using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Helpers
{
    public class CustomSettingConverter
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
                        IDropdown dropdownData = setting.CustomData as IDropdown;
                        string[] options = dropdownData.Contents.ToArray();
                        if (dropdownData.DynamicContent != DynamicContentType.None)
                        {
                            switch (dropdownData.DynamicContent)
                            {
                                case DynamicContentType.Players:
                                    options = Player.ReadyList.Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.Scps:
                                    options = Player.ReadyList.Where(p => p.Role.GetTeam() == Team.SCPs).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.ClassD:
                                    options = Player.ReadyList.Where(p => p.Role.GetTeam() == Team.ClassD).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.Scientists:
                                    options = Player.ReadyList.Where(p => p.Role.GetTeam() == Team.Scientists).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.Guards:
                                    options = Player.ReadyList.Where(p => p.Role == RoleTypeId.FacilityGuard).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.MTF:
                                    options = Player.ReadyList.Where(p => p.Role.GetTeam() == Team.FoundationForces).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.ChaosInsurgency:
                                    options = Player.ReadyList.Where(p => p.Role.GetTeam() == Team.ChaosInsurgency).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.DeadPlayers:
                                    options = Player.ReadyList.Where(p => !p.IsAlive).Select(p => p.Nickname).ToArray();
                                    break;
                                case DynamicContentType.Roles:
                                    options = System.Enum.GetNames(typeof(RoleTypeId));
                                    break;
                                case DynamicContentType.Teams:
                                    options = System.Enum.GetNames(typeof(Team));
                                    break;
                                case DynamicContentType.Items:
                                    options = System.Enum.GetNames(typeof(ItemType));
                                    break;
                                case DynamicContentType.Zones:
                                    options = System.Enum.GetNames(typeof(FacilityZone));
                                    break;
                                case DynamicContentType.Rooms:
                                    options = Room.List.Select(r => r.GameObject.name.ToString()).ToArray();
                                    break;
                                case DynamicContentType.Doors:
                                    options = Door.List.Select(d => d.DoorName.ToString()).ToArray();
                                    break;
                                case DynamicContentType.Primitives:
                                    options = AdminToy.List.Select(toy => toy.Base.name).ToArray();
                                    break;
                            }
                        }
                        serverSetting = new SSDropdownSetting(
                            (int)dropdownData.Id,
                            dropdownData.Label,
                            options,
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
