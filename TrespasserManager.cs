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

        public Settings Settings { get { return mSettings; } }
        public SandboxConfigManager SandboxConfigManager { get { return mSandboxConfigManager; } }

        public void Initialize()
        {
            mSettings = new Settings();
            mSandboxConfigManager = new SandboxConfigManager();
        }
    }
}
