using HarmonyLib;
using Il2Cpp;

namespace Trespasser
{
    internal static class MenuPatches
    {
        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.Initialize))]
        internal static class PanelSelectExperience_AddTrespasserMode
        {
            public static void Prefix(Panel_SelectExperience __instance) => TrespasserManager.Instance.MenuManager.AddTrespasserMode(__instance);

            public static void Postfix(Panel_SelectExperience __instance) => TrespasserManager.Instance.MenuManager.InitializeOverlay(__instance);
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.OnlyEnableItem))]
        internal static class PanelSelectExperience_TrespasserSelectAnim
        {
            internal static void Postfix(Panel_SelectExperience.XPModeMenuItem enabledItem) => TrespasserManager.Instance.MenuManager.OnlyEnableItem(enabledItem);
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.UpdateAnimation))]
        internal static class PanelSelectExperience_TrespasserDeselectAnim
        {
            internal static void Prefix(Panel_SelectExperience __instance) => TrespasserManager.Instance.MenuManager.UpdateAnimation(__instance);
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.IsPlayingAnyDeselectionAnimation))]
        internal static class PanelSelectExperience_HoldDuringDeselect
        {
            internal static void Postfix(ref bool __result)
            {
                if (!__result && TrespasserManager.Instance.MenuManager.IsDeselectFading)
                    __result = true;
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.IsPlayingAnySelectionAnimation))]
        internal static class PanelSelectExperience_HoldDuringSelect
        {
            internal static void Postfix(ref bool __result)
            {
                if (!__result && TrespasserManager.Instance.MenuManager.IsSelectFading)
                    __result = true;
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.HideAllAnimatedItems))]
        internal static class PanelSelectExperience_ResetTrespasserState
        {
            internal static void Postfix() => TrespasserManager.Instance.MenuManager.HideOverlay();
        }


        [HarmonyPatch(typeof(Panel_PauseMenu), nameof(Panel_PauseMenu.SetExperienceModeIcon))]
        internal static class PanelPauseMenu_UseTrespasserIcon
        {
            internal static void Postfix(Panel_PauseMenu __instance) => TrespasserManager.Instance.MenuManager.SetPauseMenuIcon(__instance);
        }


        [HarmonyPatch(typeof(Panel_ChooseSandbox), nameof(Panel_ChooseSandbox.RefreshDetails))]
        internal static class PanelChooseSandbox_TrespasserOverlay
        {
            internal static void Postfix(Panel_ChooseSandbox __instance, SaveSlotInfo slotInfo) => TrespasserManager.Instance.MenuManager.RefreshSandboxDetails(__instance, slotInfo);
        }


        [HarmonyPatch(typeof(Panel_ChooseSandbox), nameof(Panel_ChooseSandbox.Enable))]
        internal static class PanelChooseSandbox_OverlayLifecycle
        {
            internal static void Postfix(Panel_ChooseSandbox __instance, bool enable) => TrespasserManager.Instance.MenuManager.EnableSandboxPanel(__instance, enable);
        }
    }
}
