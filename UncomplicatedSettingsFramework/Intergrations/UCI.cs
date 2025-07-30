using System;
using System.Linq;
using System.Reflection;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using UncomplicatedSettingsFramework.Api.Features.Helper;

namespace UncomplicatedSettingsFramework.Integrations
{
    internal class UCI
    {       
        public static Assembly Assembly => PluginLoader.Plugins.FirstOrDefault(p => p.Key.Name is "UncomplicatedCustomItems").Value;

        public static Type Utilities => Assembly?.GetType("UncomplicatedCustomItems.API.Utilities");

        public static Type SummonedCustomItem => Assembly?.GetType("UncomplicatedCustomItems.API.Features.SummonedCustomItem");

        public static bool Available => Utilities is not null && SummonedCustomItem is not null;

        public static bool HasCustomItem(uint id, out object customItem)
        {
            customItem = null;

            if (!Available)
                return false;

            LogManager.Silent($"UCI found, trying check if the item {id} exists...");

            try
            {
                MethodInfo hasCustomItem = Utilities.GetMethod("IsCustomItem", BindingFlags.Public | BindingFlags.Static);
                MethodInfo getCustomItem = Utilities.GetMethod("GetCustomItem", BindingFlags.Public | BindingFlags.Static);

                if (hasCustomItem is not null && getCustomItem is not null)
                    if ((bool)hasCustomItem.Invoke(null, new object[] { id }))
                    {
                        customItem = getCustomItem.Invoke(null, new object[] { id });

                        return customItem is not null;
                    }

                return false;
            }
            catch (Exception e)
            {
                LogManager.Error(e.ToString());
                return false;
            }
        }

        public static void GiveCustomItem(uint id, Player player)
        {
            if (!Available)
                return;

            LogManager.Silent($"UCI found, trying to give the item {id} to {player}");

            try
            {
                if (HasCustomItem(id, out object customItem) && customItem is not null)
                {
                    SummonedCustomItem.GetConstructor(new Type[] { Assembly.GetType("UncomplicatedCustomItems.Interfaces.ICustomItem"), typeof(Player) }).Invoke(new object[] { customItem, player });
                }
            }
            catch (Exception e)
            {
                LogManager.Error(e.ToString());
            }
        }
    }
}