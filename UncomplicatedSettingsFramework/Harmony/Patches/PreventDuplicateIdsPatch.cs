using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;
using UncomplicatedSettingsFramework.Api.Features.Helper;

[HarmonyPatch(typeof(ServerSpecificSettingsSync))]
public static class PreventDuplicateIdsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToAll))]
    public static void SendToAll_Prefix() => FilterDuplicates();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
    public static void SendToPlayer_Prefix() => FilterDuplicates();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
    public static void SendToPlayersConditionally_Prefix() => FilterDuplicates();

    private static void FilterDuplicates()
    {
        ServerSpecificSettingBase[] defined = ServerSpecificSettingsSync.DefinedSettings;
        if (defined is null)
            return;

        HashSet<int> seen = [];
        ServerSpecificSettingBase[] filtered = defined.Where(setting => seen.Add(setting.SettingId)).ToArray();

        if (filtered.Length != defined.Length)
        {
            LogManager.Silent($"[SSS] Removed {defined.Length - filtered.Length} duplicate settings.");
            typeof(ServerSpecificSettingsSync).GetProperty(nameof(ServerSpecificSettingsSync.DefinedSettings)).SetValue(null, filtered);
        }
    }
}
