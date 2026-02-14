using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;

namespace Trespasser
{
    internal class Patches
    {
        private const float SELECT_DURATION = 0.7f;
        private const float DESELECT_DURATION = 0.7f;
        private const int TRESPASSER_MENU_INDEX = 3;

        private static Panel_SelectExperience.XPModeMenuItem mTrespasserMenuItem;
        private static Panel_SelectExperience.XPModeMenuItem mInterloperMenuItem;
        private static bool mHasFadedOut;


        private static bool IsSameItem(Panel_SelectExperience.XPModeMenuItem a, Panel_SelectExperience.XPModeMenuItem b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Pointer == b.Pointer;
        }


        private static void DisableTextureWidgets(GameObject display)
        {
            string[] textureNames = { "InterloperXPTexture", "InterloperXPTexture_bg" };
            foreach (string name in textureNames)
            {
                Transform child = display.transform.Find(name);
                if (child == null) continue;
                UIWidget widget = child.GetComponent<UIWidget>();
                if (widget != null)
                    widget.enabled = false;
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.Initialize))]
        internal static class PanelSelectExperience_AddTrespasserMode
        {
            public static void Prefix(ref Panel_SelectExperience __instance)
            {
                SandboxConfig trespasserConfig = SandboxConfigManager.TrespasserConfig;
                Panel_SelectExperience.XPModeMenuItem interloperItem = __instance.m_MenuItems[3];
                mInterloperMenuItem = interloperItem;
                GameObject trespasserDisplay = UnityEngine.Object.Instantiate(interloperItem.m_Display);
                trespasserDisplay.name = "TrespasserDifficultyDisplay";
                trespasserDisplay.hideFlags = HideFlags.HideAndDontSave;
                trespasserDisplay.transform.SetParent(interloperItem.m_Display.transform.parent, false);

                DisableTextureWidgets(trespasserDisplay);

                AnimationStateRef selectRef = new() { m_StateName = "" };
                AnimationStateRef deselectRef = new() { m_StateName = "" };
                Panel_SelectExperience.XPModeMenuItem trespasserItem = new()
                {
                    m_Display = trespasserDisplay,
                    m_SandboxConfig = trespasserConfig,
                    m_PlayOnSelect = selectRef,
                    m_PlayOnDeselect = deselectRef
                };
                mTrespasserMenuItem = trespasserItem;
                __instance.m_MenuItems.Insert(TRESPASSER_MENU_INDEX, trespasserItem);
            }


            public static void Postfix(Panel_SelectExperience __instance)
            {
                TrespasserOverlay.Initialize(__instance);
            }
        }

        
        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.OnlyEnableItem))]
        internal static class PanelSelectExperience_TrespasserSelectAnim
        {
            internal static void Postfix(Panel_SelectExperience.XPModeMenuItem enabledItem)
            {
                if (mTrespasserMenuItem == null)
                    return;

                if (IsSameItem(enabledItem, mTrespasserMenuItem))
                {
                    mHasFadedOut = false;
                    TrespasserOverlay.FadeIn(SELECT_DURATION);
                }
                else
                {
                    TrespasserOverlay.Hide();
                }
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.UpdateAnimation))]
        internal static class PanelSelectExperience_TrespasserDeselectAnim
        {
            internal static void Prefix(Panel_SelectExperience __instance)
            {
                if (mTrespasserMenuItem == null)
                    return;

                int state = (int)__instance.m_CurrentEpisodeAnimationState;
                if (state != 1)
                    return;

                Panel_SelectExperience.XPModeMenuItem selectedItem = __instance.GetSelectedMenuItem();
                Panel_SelectExperience.XPModeMenuItem previousItem = __instance.m_PreviousMenuItemSelected;

                if (!IsSameItem(selectedItem, mTrespasserMenuItem)
                    && IsSameItem(previousItem, mTrespasserMenuItem)
                    && !TrespasserOverlay.IsDeselectFading
                    && !mHasFadedOut)
                {
                    TrespasserOverlay.FadeOut(DESELECT_DURATION);
                    mHasFadedOut = true;
                }
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.IsPlayingAnyDeselectionAnimation))]
        internal static class PanelSelectExperience_HoldDuringDeselect
        {
            internal static void Postfix(ref bool __result)
            {
                if (__result)
                    return;

                if (TrespasserOverlay.IsDeselectFading)
                    __result = true;
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.IsPlayingAnySelectionAnimation))]
        internal static class PanelSelectExperience_HoldDuringSelect
        {
            internal static void Postfix(ref bool __result)
            {
                if (__result)
                    return;

                if (TrespasserOverlay.IsSelectFading)
                    __result = true;
            }
        }


        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.HideAllAnimatedItems))]
        internal static class PanelSelectExperience_ResetTrespasserState
        {
            internal static void Postfix()
            {
                TrespasserOverlay.Hide();
            }
        }

        [HarmonyPatch(typeof(ExperienceModeManager), nameof(ExperienceModeManager.GetGameModeFromName))]
        internal static class ExperienceModeManager_ProvideTrespasserGameModeOnGetGameModeFromName
        {
            internal static void Postfix(string gameModeName, ref GameModeConfig __result)
            {
                MelonLogger.Msg($"GameModeName: {gameModeName}");
                if (gameModeName.Contains("Trespasser"))
                {
                    __result = SandboxConfigManager.TrespasserConfig;
                }
            }
        }


        [HarmonyPatch(typeof(ExperienceModeManager), nameof(ExperienceModeManager.SetGameModeConfig))]
        internal static class ExperienceModeManager_SetGameModeConfigDebug
        {
            internal static bool Prefix(GameModeConfig gameModeConfig, ExperienceModeManager __instance)
            {
                return gameModeConfig != null
                    || ExperienceModeManager.s_CurrentGameMode == null
                    || !ExperienceModeManager.s_CurrentGameMode.m_ModeName.m_LocalizationID.Contains("Trespasser");
            }
        }


        [HarmonyPatch(typeof(DisableObjectForGameMode), nameof(DisableObjectForGameMode.ShouldDisableForCurrentMode))]
        internal static class DisableObjectForGameMode_MaybeAllowForCurrentMode
        {
            internal static void Postfix(DisableObjectForGameMode __instance, ref bool __result)
            {
                if (!__result) return;
                if (!IsTrespasserMode()) return;
                if (__instance.GetComponent<GearItem>() == null) return;

                __result = Utils.RollChance(90f);
                if (!__result)
                {
                    MelonLogger.Msg($"[Trespasser] Tag override: allowing {__instance.name} (10% roll) at {__instance.transform.position}");
                }
            }
        }


        [HarmonyPatch(typeof(DisableObjectForXPMode), nameof(DisableObjectForXPMode.ShouldDisableForCurrentMode))]
        internal static class DisableObjectForXPMode_MaybeAllowForCurrentMode
        {
            internal static void Postfix(DisableObjectForXPMode __instance, ref bool __result)
            {
                if (!__result) return;
                if (!IsTrespasserMode()) return;
                if (__instance.GetComponent<GearItem>() == null) return;

                __result = Utils.RollChance(90f);
                if (!__result)
                {
                    MelonLogger.Msg($"[Trespasser] XPMode override: allowing {__instance.name} (10% roll) at {__instance.transform.position}");
                }
            }
        }


        internal static bool IsTrespasserMode()
        {
            return ExperienceModeManager.s_CurrentGameMode != null
                && ExperienceModeManager.s_CurrentGameMode.m_ModeName.m_LocalizationID.Contains("Trespasser");
        }
    }
}
