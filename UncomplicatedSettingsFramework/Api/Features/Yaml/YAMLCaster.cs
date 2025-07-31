using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features.Helper;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Interfaces;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;
using YamlDotNet.Serialization.NamingConventions;

namespace UncomplicatedSettingsFramework.Api.Features.Yaml
{
    /// <summary>
    /// Casts the YAML data from <see cref="Customsetting"/> files into the plugin
    /// </summary>
    public static class YAMLCaster
    {
        /// <summary>
        /// Encode Data elements into YAML-friendly dictionaries with proper list handling
        /// </summary>
        public static Dictionary<string, string> Encode(Data element)
        {
            Dictionary<string, string> serialized = new();
            SnakeCaseNamingStrategy snakeCaseStrategy = new();

            foreach (PropertyInfo property in element.GetType().GetProperties())
            {
                object value = property.GetValue(element, null);
                string key = snakeCaseStrategy.GetPropertyName(property.Name, false);

                if (value == null)
                {
                    serialized.Add(key, "");
                }
                else if (value is IList list && property.PropertyType.IsGenericType)
                {
                    List<string> stringList = list.Cast<object>().Select(setting => setting?.ToString() ?? "").ToList();
                    serialized.Add(key, string.Join(",", stringList));
                }
                else if (value is Enum)
                {
                    serialized.Add(key, value.ToString());
                }
                else
                {
                    serialized.Add(key, value.ToString());
                }
            }

            return serialized;
        }

        /// <summary>
        /// Decode dictionary data back into typed objects with proper type conversion
        /// </summary>
        public static IData Decode(Data baseElement, Dictionary<string, string> data)
        {
            foreach (KeyValuePair<string, string> element in data)
            {
                PropertyInfo propertyInfo = baseElement.GetType().GetProperty(PascalCaseNamingConvention.Instance.Apply(element.Key));

                if (propertyInfo == null) continue;

                try
                {
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        object enumValue = Enum.Parse(propertyInfo.PropertyType, element.Value);
                        propertyInfo.SetValue(baseElement, enumValue, null);
                    }
                    else if (propertyInfo.PropertyType.IsGenericType &&
                             propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type elementType = propertyInfo.PropertyType.GetGenericArguments()[0];
                        Type listType = typeof(List<>).MakeGenericType(elementType);
                        IList list = Activator.CreateInstance(listType) as IList;

                        if (!string.IsNullOrEmpty(element.Value))
                        {
                            string[] settings = element.Value.Split(',');
                            foreach (string setting in settings)
                            {
                                object convertedsetting = Convert.ChangeType(setting.Trim(), elementType);
                                list?.Add(convertedsetting);
                            }
                        }

                        propertyInfo.SetValue(baseElement, list, null);
                    }
                    else
                    {
                        object convertedValue = Convert.ChangeType(element.Value, propertyInfo.PropertyType);
                        propertyInfo.SetValue(baseElement, convertedValue, null);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to set property {propertyInfo.Name}: {ex.Message}");
                }
            }

            return baseElement;
        }

        /// <summary>
        /// Create base object by SettingType and then decode the data into it
        /// </summary>
        public static Data DecodeComplete(SettingType type, Dictionary<string, string> data)
        {
            Data baseElement = CreateBaseElement(type);
            return (Data)Decode(baseElement, data);
        }

        private static Data CreateBaseElement(SettingType type)
        {
            return type switch
            {
                SettingType.Button => new Button(),
                SettingType.Dropdown => new Dropdown(),
                SettingType.Header => new Header(),
                SettingType.Keybind => new Keybind(),
                SettingType.PlainText => new PlainText(),
                SettingType.Slider => new Slider(),
                SettingType.Text => new Text(),
                SettingType.TwoButtons => new TwoButtons(),
                _ => new Data(),
            };
        }

        /// <summary>
        /// Check if dictionary contains all required keys for the given element type
        /// </summary>
        public static bool Check(IData element, Dictionary<string, string> data, out string expectedKey, out string keyList)
        {
            expectedKey = null;
            keyList = string.Join(", ", data.Keys);
            SnakeCaseNamingStrategy snakeCaseStrategy = new();

            foreach (PropertyInfo property in element.GetType().GetProperties())
            {
                string expectedSnakeKey = snakeCaseStrategy.GetPropertyName(property.Name, false);
                if (!data.ContainsKey(expectedSnakeKey))
                {
                    expectedKey = expectedSnakeKey;
                    return false;
                }
            }

            return true;
        }

        public static ICustomSetting ConvertToCustomSetting(YAMLCustomSetting yamlsetting)
        {
            if (yamlsetting?.Settings == null || yamlsetting.Settings.Count == 0)
            {
                throw new ArgumentException("The YAML setting must have at least one setting.", nameof(yamlsetting));
            }

            List<Setting> convertedSettings = [];

            foreach (YAMLSetting yamlSetting in yamlsetting.Settings)
            {
                if (yamlSetting.CustomDataList != null && yamlSetting.CustomDataList.Count > 0)
                {
                    foreach (var customData in yamlSetting.CustomDataList)
                    {
                        Data baseElement = CreateBaseElement(yamlSetting.SettingType);
                        IData decodedData = Decode(baseElement, customData);

                        convertedSettings.Add(new Setting
                        {
                            SettingType = yamlSetting.SettingType,
                            CustomData = decodedData,
                            Actions = yamlSetting.Actions
                        });
                    }
                }
                else
                {
                    Data baseElement = CreateBaseElement(yamlSetting.SettingType);

                    convertedSettings.Add(new Setting
                    {
                        SettingType = yamlSetting.SettingType,
                        CustomData = baseElement,
                        Actions = yamlSetting.Actions
                    });
                }
            }

            return new CustomSetting
            {
                Id = yamlsetting.Id,
                Name = yamlsetting.Name,
                RequiredPermission = yamlsetting.RequiredPermission,
                SpawnData = yamlsetting.SpawnData,
                Settings = convertedSettings
            };
        }
    }
}

