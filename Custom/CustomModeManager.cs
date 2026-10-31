using Il2Cpp;
using Il2CppTLD.Gameplay;
using ModData;

namespace Trespasser
{
    internal class CustomModeManager
    {
        private const string ModDataSuffix = "CustomTrespasserLevels";
        private const char Separator = ',';

        private TrespasserManager mManager;
        private CustomLevels mLevels;
        private ModDataManager mModData = new("Trespasser", false);
        private string? mLoadedSaveName;
        private bool mIsContinuing;
        private string? mHandoffCode;
        private List<string> mHandoffTunables = new();

        public CustomModeManager(TrespasserManager manager)
        {
            mManager = manager;
            mLevels = new CustomLevels(manager);
        }


        public void AddOptions(Panel_CustomXPSetup panel) => mLevels.AddOptions(panel);


        public void SuspendLevels(CustomExperienceMode customMode) => mLevels.Suspend(customMode);


        public void RestoreLevels(CustomExperienceMode customMode) => mLevels.Restore(customMode);


        public void OnUpdatedBaseValues(CustomExperienceMode customMode)
        {
            mLevels.Restore(customMode);
            mLevels.ApplyTrespasserValues(customMode);
        }


        public void BeginContinue()
        {
            ClearHandoff();
            mIsContinuing = true;
        }


        public void EndContinue() => mIsContinuing = false;


        public void OnCodeEntered() => ClearHandoff();


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
            mModData.Save(string.Join(Separator, mLevels.TrespasserTunables(customMode)), ModDataSuffix);
        }


        public void ForgetLoadedSave() => mLoadedSaveName = null;


        public void LoadLevelsOncePerSave(bool isSceneRestored)
        {
            if (!isSceneRestored) return;
            string saveName = SaveGameSystem.GetCurrentSaveName();
            if (saveName == mLoadedSaveName) return;
            mLoadedSaveName = saveName;

            CustomExperienceMode? customMode = CurrentCustomMode();
            string? stored = customMode == null ? null : mModData.Load(ModDataSuffix);
            if (string.IsNullOrEmpty(stored)) return;

            mLevels.SetToTrespasser(customMode, new HashSet<string>(stored.Split(Separator, StringSplitOptions.RemoveEmptyEntries)));
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
