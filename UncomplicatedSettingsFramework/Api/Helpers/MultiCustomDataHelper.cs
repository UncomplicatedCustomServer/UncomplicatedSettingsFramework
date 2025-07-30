using System.Collections.Generic;
using UncomplicatedSettingsFramework.Api.Enums;
using UncomplicatedSettingsFramework.Api.Features;
using UncomplicatedSettingsFramework.Api.Features.SpecificData;
using UncomplicatedSettingsFramework.Api.Features.Yaml;
using UncomplicatedSettingsFramework.Api.Interfaces.SpecificData;

namespace UncomplicatedSettingsFramework.Api.Helpers
{
    public static class MultiCustomDataHelper
    {
        public static YAMLSetting CreateMultiDataSetting(SettingType settingType, params Data[] dataObjects)
        {
            YAMLSetting setting = new()
            {
                SettingType = settingType
            };

            foreach (Data dataObject in dataObjects)
            {
                setting.CustomDataList.Add(YAMLCaster.Encode(dataObject));
            }

            return setting;
        }

        public static List<Setting> ConvertMultiDataToSettings(YAMLSetting yamlSetting)
        {
            List<Setting> settings = [];

            foreach (var customData in yamlSetting.CustomDataList)
            {
                Data baseElement = CreateBaseElement(yamlSetting.SettingType);
                IData decodedData = YAMLCaster.Decode(baseElement, customData);

                settings.Add(new Setting
                {
                    SettingType = yamlSetting.SettingType,
                    CustomData = decodedData
                });
            }

            return settings;
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
    }
}
