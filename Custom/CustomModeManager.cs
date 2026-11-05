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
        private const int LootShownIndex = 1;
        private const int PresetShownIndex = 3;

        private TrespasserManager mManager;
        private CustomLevels mLevels;
        private ModDataManager mModData = new("Trespasser", false);
        private string? mLoadedSaveName;
        private bool mIsContinuing;
        private string? mHandoffCode;
        private List<string> mHandoffTunables = new();
        private bool mLootActive;
        private bool mIsReadingFromUI;
        private bool mIsPresetLevelHidden;

        public CustomModeManager(TrespasserManager manager)
        {
            mManager = manager;
            mLevels = new CustomLevels(manager);
        }


        public void AddOptions(Panel_CustomXPSetup panel)
        {
            mLevels.AddOptions(panel);
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            if (lootCombo?.items != null && !lootCombo.items.Contains(CustomLevels.MediumLow) && lootCombo.items.Count == VanillaLootLevelCount)
                lootCombo.items.Insert(LootShownIndex, CustomLevels.MediumLow);
            ConsoleComboBox presetCombo = panel.m_XPModePopupList;
            if (presetCombo?.items != null && !presetCombo.items.Contains(CustomLevels.LevelLabel) && presetCombo.items.Count > PresetShownIndex)
                presetCombo.items.Insert(PresetShownIndex, CustomLevels.LevelLabel);
        }


        public bool ShouldSetValuesToMatchXPMode(Panel_CustomXPSetup panel)
        {
            if (panel.m_XPModePopupList?.value != CustomLevels.LevelLabel)
            {
                mLootActive = false;
                HidePresetLevel(panel);
                return true;
            }

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


        public void HidePresetLevel(Panel_CustomXPSetup panel)
        {
            ConsoleComboBox presetCombo = panel.m_XPModePopupList;
            if (presetCombo?.items == null || !presetCombo.items.Remove(CustomLevels.LevelLabel)) return;
            presetCombo.m_CurrentIndex = presetCombo.items.IndexOf(presetCombo.value);
            mIsPresetLevelHidden = true;
        }


        public void ShowPresetLevel(Panel_CustomXPSetup panel)
        {
            ConsoleComboBox presetCombo = panel.m_XPModePopupList;
            if (!mIsPresetLevelHidden || presetCombo?.items == null) return;
            mIsPresetLevelHidden = false;
            presetCombo.items.Insert(PresetShownIndex, CustomLevels.LevelLabel);
            presetCombo.m_CurrentIndex = presetCombo.items.IndexOf(presetCombo.value);
        }


        public void ShowPresetWhenMatching(Panel_CustomXPSetup panel)
        {
            ShowPresetLevel(panel);
            CustomExperienceMode customMode = panel.m_CustomMode;
            ConsoleComboBox presetCombo = panel.m_XPModePopupList;
            if (customMode == null || presetCombo?.items == null || !presetCombo.items.Contains(CustomLevels.LevelLabel)) return;
            if (mLootActive && mLevels.TrespasserTunables(customMode).Count == mLevels.Count)
                presetCombo.value = CustomLevels.LevelLabel;
        }


        public bool IsTrespasserLootActive() => mLootActive && CurrentCustomMode() != null;


        public void BeginReadFromUI(Panel_CustomXPSetup panel)
        {
            if (mIsReadingFromUI) return;
            mIsReadingFromUI = true;
            mLevels.BeginReadFromUI(panel);
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            if (lootCombo?.items == null || !lootCombo.items.Contains(CustomLevels.MediumLow)) return;
            mLootActive = lootCombo.m_CurrentIndex == LootShownIndex;
            if (lootCombo.m_CurrentIndex >= LootShownIndex) lootCombo.m_CurrentIndex--;
        }


        public void EndReadFromUI(Panel_CustomXPSetup panel)
        {
            if (!mIsReadingFromUI) return;
            mLevels.EndReadFromUI(panel);
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            if (lootCombo?.items != null)
                lootCombo.m_CurrentIndex = lootCombo.items.IndexOf(lootCombo.value);
            mIsReadingFromUI = false;
        }


        public void ShowInUI(Panel_CustomXPSetup panel)
        {
            mLevels.ShowInUI(panel);
            ConsoleComboBox lootCombo = panel.m_BaseDifficultyPopupList;
            CustomExperienceMode customMode = panel.m_CustomMode;
            if (customMode == null || lootCombo?.items == null || !lootCombo.items.Contains(CustomLevels.MediumLow)) return;
            int level = Convert.ToInt32(customMode.m_BaseResourceAvailability);
            lootCombo.value = mLootActive ? CustomLevels.MediumLow : lootCombo.items[level < LootShownIndex ? level : level + 1];
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
