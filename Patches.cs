using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gear;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private static Texture2D mCachedBwIcon;
        private static Texture2D mCachedFullColor;
        private static bool mSandboxWasTrespasser;
        private static int mHiddenSandboxAnimIndex = -1;


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
                SandboxConfig trespasserConfig = TrespasserManager.Instance.SandboxConfigManager.TrespasserConfig;
                Panel_SelectExperience.XPModeMenuItem interloperItem = __instance.m_MenuItems[3];
                mInterloperMenuItem = interloperItem;
                GameObject trespasserDisplay = UnityEngine.Object.Instantiate(interloperItem.m_Display);
                trespasserDisplay.name = "Trespasser";
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

        private static Texture2D GetCachedTexture(ref Texture2D cache, string filename)
        {
            if (cache == null)
                cache = TrespasserOverlay.LoadEmbeddedTexture(filename);
            return cache;
        }


        [HarmonyPatch(typeof(Panel_PauseMenu), nameof(Panel_PauseMenu.SetExperienceModeIcon))]
        internal static class PanelPauseMenu_UseTrespasserIcon
        {
            private static GameObject mOverlayGo;
            private static UITexture mOverlayTexture;

            internal static void Postfix(Panel_PauseMenu __instance)
            {
                MelonLogger.Msg("[Trespasser] SetExperienceModeIcon postfix fired");

                UISprite iconSprite = __instance.m_GameModeIcon;
                if (iconSprite == null)
                {
                    MelonLogger.Msg("[Trespasser] m_GameModeIcon is null");
                    return;
                }

                bool isTrespasser = TrespasserManager.Instance.SandboxConfigManager.IsTrespasserMode();
                MelonLogger.Msg($"[Trespasser] IsTrespasserMode={isTrespasser}, sprite={iconSprite.spriteName}");

                if (!isTrespasser)
                {
                    if (mOverlayGo != null)
                        mOverlayGo.SetActive(false);
                    iconSprite.enabled = true;
                    return;
                }

                Texture2D tex = GetCachedTexture(ref mCachedBwIcon, "trespasser_bw_icon.png");
                if (tex == null)
                {
                    MelonLogger.Msg("[Trespasser] Failed to load trespasser_bw_icon.png");
                    return;
                }

                iconSprite.enabled = false;

                if (mOverlayTexture == null || mOverlayGo == null)
                {
                    MelonLogger.Msg("[Trespasser] Creating pause icon overlay");
                    mOverlayGo = new GameObject("TrespasserPauseIcon");
                    mOverlayGo.transform.SetParent(iconSprite.transform.parent, false);
                    mOverlayGo.transform.localPosition = iconSprite.transform.localPosition;
                    mOverlayGo.transform.localScale = iconSprite.transform.localScale;
                    mOverlayTexture = mOverlayGo.AddComponent<UITexture>();
                    mOverlayTexture.depth = iconSprite.depth + 1;
                }

                mOverlayTexture.mainTexture = tex;
                mOverlayTexture.width = iconSprite.width;
                mOverlayTexture.height = iconSprite.height;
                mOverlayGo.SetActive(true);
                MelonLogger.Msg($"[Trespasser] Overlay active, size={iconSprite.width}x{iconSprite.height}");
            }
        }


        [HarmonyPatch(typeof(Panel_ChooseSandbox), nameof(Panel_ChooseSandbox.RefreshDetails))]
        internal static class PanelChooseSandbox_TrespasserOverlay
        {
            internal static void Postfix(Panel_ChooseSandbox __instance, SaveSlotInfo slotInfo)
            {
                bool isTrespasser = TrespasserManager.Instance.SandboxConfigManager.IsTrespasserSave(slotInfo);

                if (isTrespasser && !mSandboxWasTrespasser)
                {
                    HideActiveSandboxAnim(__instance);
                    if (TrespasserOverlay.IsInitialized)
                        TrespasserOverlay.FadeIn(SELECT_DURATION, showDesc: false);
                }
                else if (!isTrespasser && mSandboxWasTrespasser)
                {
                    RestoreSandboxAnim(__instance);
                    if (TrespasserOverlay.IsInitialized)
                        TrespasserOverlay.FadeOut(DESELECT_DURATION);
                }

                mSandboxWasTrespasser = isTrespasser;
            }
        }


        [HarmonyPatch(typeof(Panel_ChooseSandbox), nameof(Panel_ChooseSandbox.Enable))]
        internal static class PanelChooseSandbox_OverlayLifecycle
        {
            internal static void Postfix(Panel_ChooseSandbox __instance, bool enable)
            {
                if (!enable)
                {
                    RestoreSandboxAnim(__instance);
                    TrespasserOverlay.Hide();
                    mSandboxWasTrespasser = false;
                }
            }
        }


        private static void HideActiveSandboxAnim(Panel_ChooseSandbox panel)
        {
            Il2CppSystem.Collections.Generic.List<Panel_ChooseSandbox.AnimMenuItems> items = panel.m_AnimMenuItems;
            if (items == null) return;
            for (int i = 0; i < items.Count; i++)
            {
                Panel_ChooseSandbox.AnimMenuItems item = items[i];
                if (item?.m_Display != null && item.m_Display.activeSelf)
                {
                    item.m_Display.SetActive(false);
                    mHiddenSandboxAnimIndex = i;
                    return;
                }
            }
        }


        private static void RestoreSandboxAnim(Panel_ChooseSandbox panel)
        {
            if (mHiddenSandboxAnimIndex < 0) return;
            Il2CppSystem.Collections.Generic.List<Panel_ChooseSandbox.AnimMenuItems> items = panel.m_AnimMenuItems;
            if (items != null && mHiddenSandboxAnimIndex < items.Count)
            {
                Panel_ChooseSandbox.AnimMenuItems item = items[mHiddenSandboxAnimIndex];
                if (item?.m_Display != null)
                    item.m_Display.SetActive(true);
            }
            mHiddenSandboxAnimIndex = -1;
        }
    }
}
