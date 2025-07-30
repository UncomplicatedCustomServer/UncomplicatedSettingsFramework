using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Interfaces;

namespace UncomplicatedSettingsFramework.Api.Features
{
    public class ActiveCustomSetting
    {

        public static List<ActiveCustomSetting> List { get; } = [];

        public ICustomSetting CustomSetting { get; internal set; }

        public uint Id { get; internal set; }

        /// <summary>
        /// Create a new instance of <see cref="ActiveCustomSetting"/>
        /// </summary>
        /// <param name="CustomSetting"></param>
        public ActiveCustomSetting(ICustomSetting customSetting)
        {
            CustomSetting = customSetting;
            Id = customSetting.Id;
            List.Add(this);
        }

        /// <summary>
        /// Gets a <see cref="ActiveCustomSetting"/> by it's serial.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ActiveCustomSetting Get(uint id) => List.Where(sci => sci.Id == id).FirstOrDefault();

        /// <summary>
        /// Try gets a <see cref="ActiveCustomSetting"/> by it's serial.
        /// It can't be a pickup!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TryGet(uint id, out ActiveCustomSetting item)
        {
            item = Get(id);
            return item != null;
        }
    }
}
