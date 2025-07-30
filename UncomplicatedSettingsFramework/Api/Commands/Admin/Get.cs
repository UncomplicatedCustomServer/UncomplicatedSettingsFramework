using System.Collections.Generic;
using System.Text;
using CommandSystem;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.SpawnData;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Commands.Admin
{
    internal class Get : ISubcommand
    {
        public string Name { get; } = "get";
        public string Description { get; } = "Gets the specified custom setting";
        public string VisibleArgs { get; } = "<Setting ID>";
        public int RequiredArgsCount { get; } = 1;
        public string RequiredPermission { get; } = "usf.get";
        public string[] Aliases { get; } = ["g", "ge", "info"];

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "Usage: get <Setting ID>";
                return false;
            }

            if (!ushort.TryParse(arguments[0], out ushort settingId))
            {
                response = $"Invalid Setting ID: '{arguments[0]}'. Please provide a valid number.";
                return false;
            }

            if (!Utilities.TryGetCustomSetting(settingId, out ICustomSetting customSetting))
            {
                response = $"Setting ID {settingId} was not found.";
                return false;
            }

            StringBuilder sb = new();
            sb.AppendLine($"Data for '{customSetting.Name}' (ID: {customSetting.Id}):");

            sb.AppendLine("Settings:");
            if (customSetting.Settings is { Count: > 0 })
            {
                int index = 1;
                foreach (Setting setting in customSetting.Settings)
                {
                    sb.AppendLine($"  Setting #{index++}:");
                    sb.AppendLine($"    Type: {setting.SettingType}");

                    sb.AppendLine($"    CustomData:");
                    if (setting.CustomData is IData data)
                    {
                        sb.AppendLine(FormatCustomData(data));
                    }
                    else
                    {
                        sb.AppendLine("      None");
                    }

                    if (setting.Actions is { Count: > 0 })
                    {
                        sb.AppendLine("    Actions:");
                        foreach (var action in setting.Actions)
                        {
                            sb.AppendLine($"      {action.Key}: [{string.Join(", ", action.Value)}]");
                        }
                    }
                    else
                    {
                        sb.AppendLine("    Actions: None");
                    }
                }
            }
            else
            {
                sb.AppendLine("  None");
            }

            sb.AppendLine("Spawn Data:");
            if (customSetting is { SpawnData: { Count: > 0 } })
            {
                int spawnIndex = 1;
                foreach (Spawn spawn in customSetting.SpawnData)
                {
                    sb.AppendLine($"  Spawn #{spawnIndex++}:");
                    sb.AppendLine($"    SendOnJoin: {spawn.SendOnJoin}");
                    sb.AppendLine($"    SendToAll: {spawn.SendToAll}");
                    sb.AppendLine($"    RemoveOnDeath: {spawn.RemoveOnDeath}");

                    if (spawn.SendOnTeamSpawn is { Count: > 0 })
                    {
                        sb.AppendLine("    SendOnTeamSpawn:");
                        foreach (SendOnTeamSpawn sots in spawn.SendOnTeamSpawn)
                            sb.AppendLine($"      Faction: {sots.Faction}, Send: {sots.Send}");
                    }
                    else
                        sb.AppendLine("    SendOnTeamSpawn: None");

                    sb.AppendLine($"    AllowedRoles: {(spawn.AllowedRoles?.Count > 0 ? string.Join(", ", spawn.AllowedRoles) : "None")}");
                }
            }
            else
            {
                sb.AppendLine("  None");
            }

            response = sb.ToString();
            return true;
        }

        private string FormatCustomData(IData data)
        {
            StringBuilder sb = new();

            switch (data)
            {
                case IButton btn:
                    sb.AppendLine($"      [Button] Id: {btn.Id}, Label: {btn.Label}, Hint: {btn.HintDescription}, Text: {btn.ButtonText}");
                    break;

                case IDropdown dropdown:
                    sb.AppendLine($"      [Dropdown] Id: {dropdown.Id}, Label: {dropdown.Label}, Hint: {dropdown.HintDescription}, Type: {dropdown.DropdownEntryType}, Default: {dropdown.DefaultContentSelected}");
                    sb.AppendLine($"      Contents: {string.Join(", ", dropdown.Contents)}");
                    break;

                case IHeader header:
                    sb.AppendLine($"      [Header] Id: {header.Id}, Label: {header.Label}, Hint: {header.HintDescription}, ReducedPadding: {header.ReducedPadding}");
                    break;

                case IKeybind keybind:
                    sb.AppendLine($"      [Keybind] Id: {keybind.Id}, Label: {keybind.Label}, Hint: {keybind.HintDescription}, SuggestedKey: {keybind.SuggestedKey}, PreventGUI: {keybind.PreventInteractionOnGui}, AllowSpectator: {keybind.AllowSpectatorTrigger}");
                    break;

                case IPlainText text:
                    sb.AppendLine($"      [PlainText] Id: {text.Id}, SyncText: {text.SyncInputText}, Placeholder: {text.Placeholder}, ContentType: {text.ContentType}, Limit: {text.CharacterLimit}");
                    break;

                case ISlider slider:
                    sb.AppendLine($"      [Slider] Id: {slider.Id}, Label: {slider.Label}, Min: {slider.MinValue}, Max: {slider.MaxValue}, Default: {slider.DefaultValue}, Integer: {slider.Integer}, Format: {slider.ValueToStringFormat}, Display: {slider.FinalDisplayFormat}");
                    break;

                case IText area:
                    sb.AppendLine($"      [Text] Id: {area.Id}, Label: {area.Label}, Hint: {area.HintDescription}, Foldout: {area.Foldout}, Align: {area.AlignmentOptions}");
                    break;

                case ITwoButtons twoBtn:
                    sb.AppendLine($"      [TwoButtons] Id: {twoBtn.Id}, Label: {twoBtn.Label}, Hint: {twoBtn.HintDescription}, A: {twoBtn.OptionA}, B: {twoBtn.OptionB}, DefaultIsB: {twoBtn.DefaultIsB}");
                    break;

                default:
                    sb.AppendLine($"      Unknown IData Type: {data.GetType().Name}");
                    break;
            }

            return sb.ToString();
        }
    }
}