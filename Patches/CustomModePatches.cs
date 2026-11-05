using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using Il2CppTLD.Gameplay;

namespace Trespasser
{
    internal static class CustomModePatches
    {
        [HarmonyPatch(typeof(CustomExperienceMode), nameof(CustomExperienceMode.UpdateBaseValues))]
        internal static class CustomExperienceMode_ApplyTrespasserLevels
        {
            internal static void Prefix(CustomExperienceMode __instance) => TrespasserManager.Instance.CustomModeManager.SuspendLevels(__instance);

            internal static void Postfix(CustomExperienceMode __instance) => TrespasserManager.Instance.CustomModeManager.OnUpdatedBaseValues(__instance);
        }


        [HarmonyPatch(typeof(CustomExperienceMode), nameof(CustomExperienceMode.GetCougarSettings))]
        internal static class CustomExperienceMode_TrespasserCougar
        {
            internal static void Prefix(CustomExperienceMode __instance) => TrespasserManager.Instance.CustomModeManager.SuspendLevels(__instance);

            internal static void Postfix(CustomExperienceMode __instance, ref CougarSettings __result) => TrespasserManager.Instance.CustomModeManager.OnGetCougarSettings(__instance, ref __result);
        }


        [HarmonyPatch(typeof(CustomExperienceMode), nameof(CustomExperienceMode.GetProportionalReduceMaxItems))]
        internal static class CustomExperienceMode_StandInForReduceMaxItems
        {
            internal static void Prefix(CustomExperienceMode __instance) => TrespasserManager.Instance.CustomModeManager.SuspendLevels(__instance);

            internal static void Postfix(CustomExperienceMode __instance) => TrespasserManager.Instance.CustomModeManager.RestoreLevels(__instance);
        }


        [HarmonyPatch(typeof(CustomExperienceMode), nameof(CustomExperienceMode.CreateStringFromCustomMode))]
        internal static class CustomExperienceMode_StandInForSharingString
        {
            internal static void Prefix(CustomExperienceMode customMode) => TrespasserManager.Instance.CustomModeManager.SuspendLevels(customMode);

            internal static void Postfix(CustomExperienceMode customMode, string __result) => TrespasserManager.Instance.CustomModeManager.OnSharingStringCreated(customMode, __result);
        }


        [HarmonyPatch(typeof(CustomExperienceMode), nameof(CustomExperienceMode.CreateCustomModeFromString))]
        internal static class CustomExperienceMode_ApplyLevelsHandedToNewGame
        {
            internal static void Postfix(CustomExperienceMode customMode, string inputString, bool __result) => TrespasserManager.Instance.CustomModeManager.OnCustomModeCreatedFromString(customMode, inputString, __result);
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.Initialize))]
        internal static class PanelCustomXPSetup_AddTrespasserOptions
        {
            internal static void Postfix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.AddOptions(__instance);
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.SetValuesToMatchXPMode))]
        internal static class PanelCustomXPSetup_ApplyTrespasserPreset
        {
            internal static bool Prefix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.ShouldSetValuesToMatchXPMode(__instance);

            internal static void Postfix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.ShowPresetLevel(__instance);
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.SetXPModePopupListString))]
        internal static class PanelCustomXPSetup_ShowTrespasserPreset
        {
            internal static void Prefix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.HidePresetLevel(__instance);

            internal static void Postfix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.ShowPresetWhenMatching(__instance);
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.SetCustomXPSettingsFromUI))]
        internal static class PanelCustomXPSetup_ReadTrespasserLevels
        {
            internal static void Prefix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.BeginReadFromUI(__instance);

            internal static void Postfix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.EndReadFromUI(__instance);
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.SetUIFromCurrentCustomXP))]
        internal static class PanelCustomXPSetup_ShowTrespasserLevels
        {
            internal static void Postfix(Panel_CustomXPSetup __instance) => TrespasserManager.Instance.CustomModeManager.ShowInUI(__instance);
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.OnContinue))]
        internal static class PanelCustomXPSetup_HandLevelsToNewGame
        {
            internal static void Prefix() => TrespasserManager.Instance.CustomModeManager.BeginContinue();

            internal static void Finalizer() => TrespasserManager.Instance.CustomModeManager.EndContinue();
        }


        [HarmonyPatch(typeof(Panel_CustomXPSetup), nameof(Panel_CustomXPSetup.OnEnterCodePopupConfirmed))]
        internal static class PanelCustomXPSetup_CodeClearsTrespasserLevels
        {
            internal static void Prefix() => TrespasserManager.Instance.CustomModeManager.OnCodeEntered();
        }


        [HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.WriteSlotToDisk), new[] { typeof(SlotData), typeof(SaveGameSlots.Timestamp) })]
        internal static class SaveGameSlots_SaveTrespasserLevels
        {
            internal static void Prefix() => TrespasserManager.Instance.CustomModeManager.SaveLevels();
        }


        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.RestoreGame))]
        internal static class SaveGameSystem_ForgetLoadedSave
        {
            internal static void Prefix() => TrespasserManager.Instance.CustomModeManager.ForgetLoadedSave();
        }


        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.LoadSceneData))]
        internal static class SaveGameSystem_LoadTrespasserLevels
        {
            internal static void Postfix(bool __result) => TrespasserManager.Instance.CustomModeManager.LoadLevelsOncePerSave(__result);
        }
    }
}
