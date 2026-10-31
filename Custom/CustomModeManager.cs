using Il2Cpp;
using Il2CppTLD.AI;
using Il2CppTLD.Gameplay;
using ModData;

namespace Trespasser
{
    internal class CustomModeManager
    {
        private const string ModDataSuffix = "CustomTrespasserLevels";
        private const char Separator = ',';
        private const string LootToken = "BaseResourceAvailability";
        private const int VanillaLootLevelCount = 4;
        private const int InterloperBaseCodeIndex = 3;

        private TrespasserManager mManager;
        private CustomLevels mLevels;
        private ModDataManager mModData = new("Trespasser", false);
        private string? mLoadedSaveName;
        private bool mIsContinuing;
        private string? mHandoffCode;
        private List<string> mHandoffTunables = new();
        private bool mLootActive;
        private bool mIsTranslatingLootCombo;

        public CustomModeManager(TrespasserManager manager)
        {
            mManager = manager;
            mLevels = new CustomLevels(manager);
        }


        public void AddOptions(Panel_CustomXPSetup panel)
        {
            mLevels.AddOptions(panel);
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            if (lootCombo?.items != null && !lootCombo.items.Contains(CustomLevels.LevelLabel) && lootCombo.items.Count == VanillaLootLevelCount)
                lootCombo.items.Add(CustomLevels.LevelLabel);
            ConsoleComboBox presetCombo = panel.m_XPModePopupList;
            if (presetCombo?.items != null && !presetCombo.items.Contains(CustomLevels.LevelLabel))
                presetCombo.items.Add(CustomLevels.LevelLabel);
        }


        public bool ShouldSetValuesToMatchXPMode(Panel_CustomXPSetup panel)
        {
            if (panel.m_XPModePopupList?.value != CustomLevels.LevelLabel) return true;

            CustomExperienceMode customMode = panel.m_CustomMode;
            string[] baseCodes = panel.m_BaseXPModeCustomStrings;
            if (customMode == null || baseCodes == null || baseCodes.Length <= InterloperBaseCodeIndex) return true;

            CustomExperienceMode.CreateCustomModeFromString(customMode, baseCodes[InterloperBaseCodeIndex]);
            mLevels.SetAllToTrespasser(customMode);
            mLootActive = true;
            customMode.UpdateBaseValues();
            panel.SetUIFromCurrentCustomXP();
            return false;
        }


        public void ShowPresetWhenMatching(Panel_CustomXPSetup panel)
        {
            CustomExperienceMode customMode = panel.m_CustomMode;
            ConsoleComboBox presetCombo = panel.m_XPModePopupList;
            if (customMode == null || presetCombo?.items == null || !presetCombo.items.Contains(CustomLevels.LevelLabel)) return;
            if (mLootActive && mLevels.TrespasserTunables(customMode).Count == mLevels.Count)
                presetCombo.value = CustomLevels.LevelLabel;
        }


        public bool IsTrespasserLootActive() => mLootActive && CurrentCustomMode() != null;


        public void BeginReadLootFromUI(Panel_CustomXPSetup panel)
        {
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            if (lootCombo == null || mIsTranslatingLootCombo) return;
            mLootActive = lootCombo.value == CustomLevels.LevelLabel;
            if (!mLootActive) return;
            mIsTranslatingLootCombo = true;
            lootCombo.value = lootCombo.items[0];
        }


        public void EndReadLootFromUI(Panel_CustomXPSetup panel)
        {
            if (!mIsTranslatingLootCombo) return;
            panel.m_BaseDifficultyPopupList.value = CustomLevels.LevelLabel;
            mIsTranslatingLootCombo = false;
        }


        public void ShowLootInUI(Panel_CustomXPSetup panel)
        {
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            if (mLootActive && lootCombo?.items != null && lootCombo.items.Contains(CustomLevels.LevelLabel))
                lootCombo.value = CustomLevels.LevelLabel;
        }


        public void SuspendLevels(CustomExperienceMode customMode) => mLevels.Suspend(customMode);


        public void RestoreLevels(CustomExperienceMode customMode) => mLevels.Restore(customMode);


        public void OnUpdatedBaseValues(CustomExperienceMode customMode)
        {
            mLevels.Restore(customMode);
            mLevels.ApplyTrespasserValues(customMode);
        }


        public void OnGetCougarSettings(CustomExperienceMode customMode, ref CougarSettings cougarSettings)
        {
            mLevels.Restore(customMode);
            CougarSettings? trespasserCougar = mLevels.TrespasserCougarSettings(customMode);
            if (trespasserCougar != null)
                cougarSettings = trespasserCougar;
        }


        public void BeginContinue()
        {
            ClearHandoff();
            mIsContinuing = true;
        }


        public void EndContinue() => mIsContinuing = false;


        public void OnCodeEntered()
        {
            mLootActive = false;
            ClearHandoff();
        }


        public void OnSharingStringCreated(CustomExperienceMode customMode, string code)
        {
            mLevels.Restore(customMode);
            if (!mIsContinuing || customMode == null || string.IsNullOrEmpty(code)) return;
            mHandoffCode = code;
            mHandoffTunables = mLevels.TrespasserTunables(customMode);
        }


        public void OnCustomModeCreatedFromString(CustomExperienceMode customMode, string code, bool decoded)
        {
            if (!decoded || customMode == null || mHandoffCode == null || code != mHandoffCode) return;
            mLevels.SetToTrespasser(customMode, mHandoffTunables);
            customMode.UpdateBaseValues();
            ClearHandoff();
        }


        public void SaveLevels()
        {
            CustomExperienceMode? customMode = CurrentCustomMode();
            if (customMode == null) return;
            List<string> trespasserTunables = mLevels.TrespasserTunables(customMode);
            if (mLootActive)
                trespasserTunables.Add(LootToken);
            mModData.Save(string.Join(Separator, trespasserTunables), ModDataSuffix);
        }


        public void ForgetLoadedSave() => mLoadedSaveName = null;


        public void LoadLevelsOncePerSave(bool isSceneRestored)
        {
            if (!isSceneRestored) return;
            string saveName = SaveGameSystem.GetCurrentSaveName();
            if (saveName == mLoadedSaveName) return;
            mLoadedSaveName = saveName;
            mLootActive = false;

            CustomExperienceMode? customMode = CurrentCustomMode();
            string? stored = customMode == null ? null : mModData.Load(ModDataSuffix);
            if (string.IsNullOrEmpty(stored)) return;

            HashSet<string> trespasserTunables = new(stored.Split(Separator, StringSplitOptions.RemoveEmptyEntries));
            mLootActive = trespasserTunables.Contains(LootToken);
            mLevels.SetToTrespasser(customMode, trespasserTunables);
            customMode.UpdateBaseValues();
        }


        private CustomExperienceMode? CurrentCustomMode()
        {
            ExperienceModeManager manager = GameManager.GetExperienceModeManagerComponent();
            return manager != null && manager.InCustomMode() ? manager.GetCustomMode() : null;
        }


        private void ClearHandoff()
        {
            mHandoffCode = null;
            mHandoffTunables.Clear();
        }
    }
}
