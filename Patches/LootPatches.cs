using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;

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
            internal static void Postfix(Container __instance) => TrespasserManager.Instance.LootManager.MaybeThinBunkerContents(__instance);
        }


        [HarmonyPatch(typeof(RandomSpawnObject), nameof(RandomSpawnObject.Start))]
        internal static class RandomSpawnObject_FixInterloperQty
        {
            internal static void Prefix(RandomSpawnObject __instance) => TrespasserManager.Instance.LootManager.FixInterloperQuantity(__instance);
        }
    }
}
