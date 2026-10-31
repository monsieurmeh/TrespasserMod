namespace Trespasser
{
    internal class TrespasserManager
    {
        #region Lazy Singleton

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly TrespasserManager instance = new TrespasserManager();
        }

        private TrespasserManager() { }
        public static TrespasserManager Instance { get { return Nested.instance; } }

        #endregion

        private Settings mSettings;
        private SandboxConfigManager mSandboxConfigManager;
        private MenuManager mMenuManager;
        private LootManager mLootManager;
        private CustomModeManager mCustomModeManager;

        public Settings Settings { get { return mSettings; } }
        public SandboxConfigManager SandboxConfigManager { get { return mSandboxConfigManager; } }
        public MenuManager MenuManager { get { return mMenuManager; } }
        public LootManager LootManager { get { return mLootManager; } }
        public CustomModeManager CustomModeManager { get { return mCustomModeManager; } }

        public void Initialize()
        {
            mSettings = new Settings();
            mSandboxConfigManager = new SandboxConfigManager();
            mMenuManager = new MenuManager(this);
            mLootManager = new LootManager(this);
            mCustomModeManager = new CustomModeManager(this);
        }
    }
}
