using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlayerRoles;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features.SpawnData;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Features.Yaml;
using UncomplicatedSettingsFramework.Api.Helpers;
using YamlDotNet.Core;
using static UserSettings.ServerSpecific.SSDropdownSetting;

namespace UncomplicatedSettingsFramework.Api.Features.Helper
{
    public class FileConfig
    {
        public static readonly List<YAMLCustomSetting> _examples =
        [
            new()
            {
                Id = 2,
                Name = "Multi-Data Test",
                SpawnData =
                [
                    new Spawn()
                    {
                        SendOnJoin = true,
                        AllowedRoles = [ RoleTypeId.ClassD ],
                        SendOnTeamSpawn =
                        [
                            new SendOnTeamSpawn
                            {
                                Faction = Faction.FoundationStaff,
                                Send = true
                            }
                        ]
                    }
                ],
                Settings =
                [
                    MultiCustomDataHelper.CreateMultiDataSetting(
                    SettingType.Header,
                    new Header()
                    {
                        Id = 3,
                        Label = "Example Header",
                        HintDescription = "This is a header for the settings below."
                    }
                ),

                    new YAMLSetting
                    {
                        SettingType = SettingType.Button,
                        CustomDataList =
                        [
                            YAMLCaster.Encode(new Button()
                            {
                                Id = 6,
                                Label = "Click Me!",
                                ButtonText = "Press"
                            })
                        ],
                        Actions = new Dictionary<string, List<string>>
                        {
                            {
                                "OnClicked",
                                [
                                    "HINT 5 Hello {player.name}!"
                                ]
                            }
                        }
                    },

                    MultiCustomDataHelper.CreateMultiDataSetting(
                        SettingType.Dropdown,
                        new Dropdown()
                        {
                            Id = 4,
                            Label = "Primary Option",
                            HintDescription = "This is a test data setting.",
                            DropdownEntryType = DropdownEntryType.Regular,
                            Contents = new List<string> { "A", "B", "C" }
                        },
                        new Dropdown()
                        {
                            Id = 5,
                            Label = "Secondary Option",
                            HintDescription = "This is a secondary test data setting.",
                            DropdownEntryType = DropdownEntryType.Hybrid,
                            Contents = new List<string> { "X", "Y", "Z" }
                        }
                    )
                ]
            }
        ];

        internal string Dir = Path.Combine(LabApi.Loader.Features.Paths.PathManager.Configs.ToString(), "UncomplicatedSettingsFramework");

        public bool Is(string localDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, localDir));
        }

        public string[] List(string localDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, localDir));
        }

        public void LoadAll(string localDir = "")
        {
            LoadAction((YAMLCustomSetting setting) =>
            {
                CustomSetting.Register(YAMLCaster.ConvertToCustomSetting(setting));
            }, localDir);
        }

        public void LoadAction(Action<YAMLCustomSetting> action, string localDir = "")
        {
            foreach (string FileName in List(localDir))
            {
                try
                {
                    if (Directory.Exists(FileName))
                        continue;

                    if (FileName.Split().First() == ".")
                        return;

                    string fileContent = File.ReadAllText(FileName);

                    try
                    {
                        YAMLCustomSetting setting = LabApi.Loader.Features.Yaml.YamlConfigParser.Deserializer.Deserialize<YAMLCustomSetting>(fileContent);
                        LogManager.Debug($"Proposed to the registerer the external setting {setting.Id} [{setting.Name}] from file:\n{FileName}");
                        action(setting);
                    }
                    catch (YamlException yamlEx)
                    {
                        string errorMessage = $"Failed to parse {FileName}. YAML syntax error: {yamlEx.Message}";

                        if (yamlEx.Start.Line > 0)
                        {
                            errorMessage += $" at line {yamlEx.Start.Line}, column {yamlEx.Start.Column}";

                            string[] lines = fileContent.Split('\n');
                            if (yamlEx.Start.Line <= lines.Length)
                            {
                                string problematicLine = lines[yamlEx.Start.Line - 1];
                                errorMessage += $"\nProblematic line: \"{problematicLine.Trim()}\"";
                            }
                        }

                        if (Plugin.Instance.Config.Debug)
                        {
                            LogManager.Error($"{errorMessage}\nStack trace: {yamlEx.StackTrace}\nIf this was caused by a plugin update you can update your CustomSetting here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                        else
                        {
                            LogManager.Error($"{errorMessage}\nIf this was caused by a plugin update you can update your CustomSetting here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Failed to process {FileName}. Error: {ex.Message}";

                        if (ex.Message.Contains("type") || ex.Message.Contains("convert"))
                        {
                            errorMessage += "\nThis appears to be a type mismatch error. Check that your values match the expected types for each field.";
                        }
                        else if (ex.Message.Contains("property") || ex.Message.Contains("member"))
                        {
                            errorMessage += "\nThis appears to be related to an unknown property. Check for typos in your YAML field names.";
                        }

                        if (Plugin.Instance.Config.Debug)
                        {
                            LogManager.Error($"{errorMessage}\nStack trace: {ex.StackTrace}\nIf this was caused by a plugin update you can update your CustomSetting here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                        else
                        {
                            LogManager.Error($"{errorMessage}\nIf this was caused by a plugin update you can update your CustomSetting here: https://uci.thaumielscpsl.site/uciupdater");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to access file {FileName}. Error: {ex.Message}\n{ex.HResult}");
                    if (Plugin.Instance.Config.Debug)
                    {
                        LogManager.Error($"Stack trace: {ex.StackTrace}");
                    }
                }
            }
        }

        public void LoadSettings(string localDir = "", bool loadExamples = false)
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));
                if (!loadExamples)
                    File.WriteAllText(Path.Combine(Dir, localDir, "example-setting.yml"), LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(new YAMLCustomSetting()
                    {
                        Id = CustomSetting.GetFirstFreeId(1)
                    }));
                else
                    foreach (YAMLCustomSetting CustomSetting in _examples)
                        File.WriteAllText(Path.Combine(Dir, localDir, $"{CustomSetting.Name.ToLower().Replace(" ", "-")}.yml"), LabApi.Loader.Features.Yaml.YamlConfigParser.Serializer.Serialize(CustomSetting));

                LogManager.Info($"Plugin does not have a setting folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }
    }
}
