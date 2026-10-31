using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.IntBackedUnit;
using Il2CppTLD.Stats;

namespace Trespasser
{
    internal static class StatsPatches
    {
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.SetValue))]
        internal static class StatsManager_RecordSetValue
        {
            internal static void Postfix(StatID id) => TrespasserManager.Instance.Stats.Record(id);
        }


        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.IncrementValue), new[] { typeof(StatID), typeof(float) })]
        internal static class StatsManager_RecordIncrementValue
        {
            internal static void Postfix(StatID id) => TrespasserManager.Instance.Stats.Record(id);
        }


        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.IncrementValue), new[] { typeof(StatID), typeof(ItemWeight) })]
        internal static class StatsManager_RecordIncrementWeight
        {
            internal static void Postfix(StatID id) => TrespasserManager.Instance.Stats.Record(id);
        }


        [HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.WriteSlotToDisk), new[] { typeof(SlotData), typeof(SaveGameSlots.Timestamp) })]
        internal static class SaveGameSlots_SaveTrespasserStats
        {
            internal static void Prefix() => TrespasserManager.Instance.Stats.SaveIfChanged();
        }


        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.DeserializeAllTimeStats))]
        internal static class StatsManager_LoadTrespasserStats
        {
            internal static void Postfix() => TrespasserManager.Instance.Stats.Load();
        }


        [HarmonyPatch(typeof(DetailedStatsView), nameof(DetailedStatsView.OnResetAllTimeStatsConfirmed))]
        internal static class DetailedStatsView_ResetTrespasserStats
        {
            internal static void Postfix() => TrespasserManager.Instance.Stats.Reset();
        }


        [HarmonyPatch(typeof(DetailedStatsView), nameof(DetailedStatsView.Populate))]
        internal static class DetailedStatsView_AddTrespasserColumn
        {
            internal static void Postfix(DetailedStatsView __instance) => TrespasserManager.Instance.MenuManager.ApplyStatsColumn(__instance);
        }
    }
}
