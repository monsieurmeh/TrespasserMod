using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Trader;
using UnityEngine;

namespace Trespasser
{
    internal static class LootPatches
    {
        [HarmonyPatch(typeof(DisableObjectForGameMode), nameof(DisableObjectForGameMode.ShouldDisableForCurrentMode))]
        internal static class DisableObjectForGameMode_MaybeAllowForCurrentMode
        {
            internal static void Postfix(DisableObjectForGameMode __instance, ref bool __result)
            {
                if (__result && TrespasserManager.Instance.LootManager.ShouldAllowBannedItem(__instance))
                    __result = false;
            }
        }


        [HarmonyPatch(typeof(DisableObjectForXPMode), nameof(DisableObjectForXPMode.ShouldDisableForCurrentMode))]
        internal static class DisableObjectForXPMode_MaybeAllowForCurrentMode
        {
            internal static void Postfix(DisableObjectForXPMode __instance, ref bool __result)
            {
                if (__result && TrespasserManager.Instance.LootManager.ShouldAllowBannedItem(__instance))
                    __result = false;
            }
        }


        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.LoadSceneData))]
        internal static class SaveGameSystem_TrackFirstVisit
        {
            internal static void Postfix(bool __result) => TrespasserManager.Instance.LootManager.OnLoadSceneData(__result);
        }


        [HarmonyPatch(typeof(GearItem), nameof(GearItem.ManualStart))]
        internal static class GearItem_MaybeRemoveFromBunker
        {
            internal static void Postfix(GearItem __instance) => TrespasserManager.Instance.LootManager.MaybeRemoveFromBunker(__instance);
        }


        [HarmonyPatch(typeof(Container), nameof(Container.Start))]
        internal static class Container_MaybeThinBunkerContents
        {
            internal static void Prefix() => TrespasserManager.Instance.LootManager.BeginContainerPopulation();

            internal static void Postfix(Container __instance) => TrespasserManager.Instance.LootManager.MaybeThinBunkerContents(__instance);
        }


        [HarmonyPatch(typeof(Container), nameof(Container.PopulateWithScriptedGear))]
        internal static class Container_RollScriptedBannedGear
        {
            internal static void Prefix(Container __instance) => TrespasserManager.Instance.LootManager.BeginPrefabRolls(LootSource.Container, __instance);

            internal static void Finalizer() => TrespasserManager.Instance.LootManager.EndPrefabRolls();
        }


        [HarmonyPatch(typeof(Container), nameof(Container.PopulateWithRandomGear))]
        internal static class Container_RollRandomBannedGear
        {
            internal static void Prefix(Container __instance, out int __state) => __state = __instance.m_GearToInstantiate?.Count ?? 0;

            internal static void Postfix(Container __instance, int __state) => TrespasserManager.Instance.LootManager.RollRandomContainerGear(__instance, __state);
        }


        [HarmonyPatch(typeof(RadialObjectSpawner), nameof(RadialObjectSpawner.GetNextPrefabToSpawn))]
        internal static class RadialObjectSpawner_RollBannedDraw
        {
            internal static void Postfix(RadialObjectSpawner __instance, ref GameObject __result) => TrespasserManager.Instance.LootManager.RollRadialDraw(__instance, ref __result);
        }


        [HarmonyPatch(typeof(RadialObjectSpawner), nameof(RadialObjectSpawner.SpawnAtPosition))]
        internal static class RadialObjectSpawner_KeepRolledBannedSpawn
        {
            internal static void Postfix(RadialObjectSpawner __instance, GameObject prefabToSpawn, GameObject __result) => TrespasserManager.Instance.LootManager.KeepRolledRadialSpawn(__instance, prefabToSpawn, __result);
        }


        [HarmonyPatch(typeof(TradeExchange), nameof(TradeExchange.GetRandomTradeExchangeData))]
        internal static class TradeExchange_RollBannedTrade
        {
            internal static void Postfix(TradeExchange __instance, TraderState currentTraderState, ref TradeExchangeData __result) => TrespasserManager.Instance.LootManager.RollTrade(__instance, currentTraderState, ref __result);
        }


        [HarmonyPatch(typeof(TradeExchange), nameof(TradeExchange.FilterExchangeData))]
        internal static class TradeExchange_ForgetBannedTrades
        {
            internal static void Postfix(TradeExchange __instance) => TrespasserManager.Instance.LootManager.OnExchangeFiltered(__instance);
        }


        [HarmonyPatch(typeof(TraderManager), nameof(TraderManager.GetAvailableTradeExchanges))]
        internal static class TraderManager_KeepRolledBannedTrades
        {
            internal static void Prefix() => TrespasserManager.Instance.LootManager.LiftBans();

            internal static void Finalizer() => TrespasserManager.Instance.LootManager.RestoreBans();
        }


        [HarmonyPatch(typeof(RandomSpawnObject), nameof(RandomSpawnObject.Start))]
        internal static class RandomSpawnObject_FixInterloperQty
        {
            internal static void Prefix(RandomSpawnObject __instance) => TrespasserManager.Instance.LootManager.FixInterloperQuantity(__instance);
        }
    }
}
