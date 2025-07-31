using System;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace UncomplicatedSettingsFramework.Api.Features.Extensions
{
    /// <summary>
    /// Provides extension methods to extract the underlying ServerSpecificSettingBase
    /// from any wrapper class that exposes a "Base" property, or cast directly.
    /// Supports all SS* settings (e.g., SSButton, SSDropdownSetting) as they derive from ServerSpecificSettingBase.
    /// </summary>
    public static class ExiledSettingBaseExtensions
    {
        /// <summary>
        /// Extracts the ServerSpecificSettingBase from an object with a "Base" property,
        /// or returns the object directly if it's already a ServerSpecificSettingBase.
        /// </summary>
        /// <param name="maybeWrapper">An object which may wrap a ServerSpecificSettingBase</param>
        /// <returns>The extracted ServerSpecificSettingBase instance.</returns>
        public static ServerSpecificSettingBase ToServerSpecific(this object maybeWrapper)
        {
            if (maybeWrapper == null)
                throw new ArgumentNullException(nameof(maybeWrapper));

            if (maybeWrapper is ServerSpecificSettingBase serverSetting)
                return serverSetting;

            PropertyInfo baseProp = maybeWrapper.GetType().GetProperty("Base", BindingFlags.Public | BindingFlags.Instance);

            if (baseProp != null)
            {
                object baseValue = baseProp.GetValue(maybeWrapper);
                if (baseValue is ServerSpecificSettingBase setting)
                    return setting;
            }

            throw new ArgumentException("Object does not expose a Base property or is not a ServerSpecificSettingBase", nameof(maybeWrapper));
        }
    }
}
