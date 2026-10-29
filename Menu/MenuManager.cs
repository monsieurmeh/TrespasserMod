using Il2Cpp;
using Il2CppTLD.Gameplay;
using MelonLoader;
using UnityEngine;

namespace Trespasser
{
    internal class MenuManager
    {
        private const float SELECT_DURATION = 0.7f;
        private const float DESELECT_DURATION = 0.7f;
        private const int TRESPASSER_MENU_INDEX = 3;

        private TrespasserManager mManager;
        private TrespasserOverlay mOverlay = new TrespasserOverlay();
        private Panel_SelectExperience.XPModeMenuItem mTrespasserMenuItem;
        private Panel_SelectExperience.XPModeMenuItem mInterloperMenuItem;
        private bool mHasFadedOut;
        private Texture2D mCachedBwIcon;
        private bool mSandboxWasTrespasser;
        private int mHiddenSandboxAnimIndex = -1;
        private GameObject mPauseIconObject;
        private UITexture mPauseIconTexture;

        public MenuManager(TrespasserManager manager)
        {
            mManager = manager;
        }

        public bool IsSelectFading => mOverlay.IsSelectFading;
        public bool IsDeselectFading => mOverlay.IsDeselectFading;


        #region Select Experience

        public void AddTrespasserMode(Panel_SelectExperience panel)
        {
            SandboxConfig trespasserConfig = mManager.SandboxConfigManager.TrespasserConfig;
            Panel_SelectExperience.XPModeMenuItem interloperItem = panel.m_MenuItems[3];
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
            panel.m_MenuItems.Insert(TRESPASSER_MENU_INDEX, trespasserItem);
        }


        public void InitializeOverlay(Panel_SelectExperience panel)
        {
            mOverlay.Initialize(panel);
        }


        public void OnlyEnableItem(Panel_SelectExperience.XPModeMenuItem enabledItem)
        {
            if (mTrespasserMenuItem == null)
                return;

            if (IsSameItem(enabledItem, mTrespasserMenuItem))
            {
                mHasFadedOut = false;
                mOverlay.FadeIn(SELECT_DURATION);
            }
            else
            {
                mOverlay.Hide();
            }
        }


        public void UpdateAnimation(Panel_SelectExperience panel)
        {
            if (mTrespasserMenuItem == null)
                return;

            int state = (int)panel.m_CurrentEpisodeAnimationState;
            if (state != 1)
                return;

            Panel_SelectExperience.XPModeMenuItem selectedItem = panel.GetSelectedMenuItem();
            Panel_SelectExperience.XPModeMenuItem previousItem = panel.m_PreviousMenuItemSelected;

            if (!IsSameItem(selectedItem, mTrespasserMenuItem)
                && IsSameItem(previousItem, mTrespasserMenuItem)
                && !mOverlay.IsDeselectFading
                && !mHasFadedOut)
            {
                mOverlay.FadeOut(DESELECT_DURATION);
                mHasFadedOut = true;
            }
        }


        public void HideOverlay()
        {
            mOverlay.Hide();
        }


        private bool IsSameItem(Panel_SelectExperience.XPModeMenuItem a, Panel_SelectExperience.XPModeMenuItem b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Pointer == b.Pointer;
        }


        private void DisableTextureWidgets(GameObject display)
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

        #endregion


        #region Choose Sandbox

        public void RefreshSandboxDetails(Panel_ChooseSandbox panel, SaveSlotInfo slotInfo)
        {
            bool isTrespasser = mManager.SandboxConfigManager.IsTrespasserSave(slotInfo);

            if (isTrespasser && !mSandboxWasTrespasser)
            {
                HideActiveSandboxAnim(panel);
                if (mOverlay.IsInitialized)
                    mOverlay.FadeIn(SELECT_DURATION, showDesc: false);
            }
            else if (!isTrespasser && mSandboxWasTrespasser)
            {
                RestoreSandboxAnim(panel);
                if (mOverlay.IsInitialized)
                    mOverlay.FadeOut(DESELECT_DURATION);
            }

            mSandboxWasTrespasser = isTrespasser;
        }


        public void EnableSandboxPanel(Panel_ChooseSandbox panel, bool enable)
        {
            if (!enable)
            {
                RestoreSandboxAnim(panel);
                mOverlay.Hide();
                mSandboxWasTrespasser = false;
            }
        }


        private void HideActiveSandboxAnim(Panel_ChooseSandbox panel)
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


        private void RestoreSandboxAnim(Panel_ChooseSandbox panel)
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

        #endregion


        #region Pause Menu

        public void SetPauseMenuIcon(Panel_PauseMenu panel)
        {
            MelonLogger.Msg("[Trespasser] SetExperienceModeIcon postfix fired");

            UISprite iconSprite = panel.m_GameModeIcon;
            if (iconSprite == null)
            {
                MelonLogger.Msg("[Trespasser] m_GameModeIcon is null");
                return;
            }

            bool isTrespasser = mManager.SandboxConfigManager.IsTrespasserMode();
            MelonLogger.Msg($"[Trespasser] IsTrespasserMode={isTrespasser}, sprite={iconSprite.spriteName}");

            if (!isTrespasser)
            {
                if (mPauseIconObject != null)
                    mPauseIconObject.SetActive(false);
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

            if (mPauseIconTexture == null || mPauseIconObject == null)
            {
                MelonLogger.Msg("[Trespasser] Creating pause icon overlay");
                mPauseIconObject = new GameObject("TrespasserPauseIcon");
                mPauseIconObject.transform.SetParent(iconSprite.transform.parent, false);
                mPauseIconObject.transform.localPosition = iconSprite.transform.localPosition;
                mPauseIconObject.transform.localScale = iconSprite.transform.localScale;
                mPauseIconTexture = mPauseIconObject.AddComponent<UITexture>();
                mPauseIconTexture.depth = iconSprite.depth + 1;
            }

            mPauseIconTexture.mainTexture = tex;
            mPauseIconTexture.width = iconSprite.width;
            mPauseIconTexture.height = iconSprite.height;
            mPauseIconObject.SetActive(true);
            MelonLogger.Msg($"[Trespasser] Overlay active, size={iconSprite.width}x{iconSprite.height}");
        }


        private Texture2D GetCachedTexture(ref Texture2D cache, string filename)
        {
            if (cache == null)
                cache = mOverlay.LoadEmbeddedTexture(filename);
            return cache;
        }

        #endregion
    }
}
