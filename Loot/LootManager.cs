using Il2Cpp;
using Il2CppTLD.Gameplay;
using MelonLoader;
using UnityEngine;

namespace Trespasser
{
    internal class LootManager
    {
        private TrespasserManager mManager;
        private bool mIsSceneRestored;

        public LootManager(TrespasserManager manager)
        {
            mManager = manager;
        }


        public void OnLoadSceneData(bool isSceneRestored)
        {
            mIsSceneRestored = isSceneRestored;
        }


        public bool ShouldAllowBannedItem(Component instance)
        {
            if (instance == null) return false;
            if (mIsSceneRestored) return false;
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return false;
            if (instance.GetComponent<GearItem>() == null) return false;
            var roll = new System.Random().NextDouble();
            bool shouldAllow = roll <= (mManager.Settings.InterloperBannedSpawnChance * 0.01f);
            if (shouldAllow && !string.IsNullOrEmpty(instance.gameObject.scene.name))
                MelonLogger.Msg($"Allowing spawn of {instance.name} at {instance.gameObject.transform.position}");
            return shouldAllow;
        }


        public void MaybeRemoveFromBunker(GearItem gearItem)
        {
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return;
            if (mIsSceneRestored) return;
            if (!IsBunkerScene(gearItem.gameObject.scene.name)) return;
            if (!gearItem.gameObject.activeSelf) return;
            if (!ShouldRemoveBunkerItem()) return;

            gearItem.gameObject.SetActive(false);
            MelonLogger.Msg($"[Trespasser] Removed bunker item: {gearItem.name}");
        }


        public void MaybeThinBunkerContents(Container container)
        {
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return;
            if (mIsSceneRestored) return;

            string sceneName = container.gameObject.scene.name;
            if (!IsBunkerScene(sceneName)) return;

            Il2CppSystem.Collections.Generic.List<string> gearList = container.m_GearToInstantiate;
            if (gearList == null || gearList.Count == 0) return;

            int removed = 0;
            for (int i = gearList.Count - 1; i >= 0; i--)
            {
                if (ShouldRemoveBunkerItem())
                {
                    gearList.RemoveAt(i);
                    removed++;
                }
            }

            if (removed > 0)
                MelonLogger.Msg($"[Trespasser] Thinned bunker container in {sceneName}: removed {removed} item(s), {gearList.Count} remaining");
        }


        public void FixInterloperQuantity(RandomSpawnObject spawner)
        {
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return;

            if (spawner.m_NumObjectsToEnableByLevel == null || spawner.m_NumObjectsToEnableByLevel.Length < 2) return;
            int currentIndex = GetCurrentResourceIndex();
            if (currentIndex < 0 || currentIndex >= spawner.m_NumObjectsToEnableByLevel.Length) return;
            if (spawner.m_NumObjectsToEnableByLevel[currentIndex] != 0) return;
            if (spawner.m_NumObjectsToEnableByLevel[currentIndex + 1] == 0) return;
            spawner.m_NumObjectsToEnableByLevel[currentIndex] = Math.Max(1, (int)(spawner.m_NumObjectsToEnableByLevel[currentIndex + 1] * 0.5));
        }


        private int GetCurrentResourceIndex()
        {
            GameModeConfig current = ExperienceModeManager.s_CurrentGameMode;
            if (current == null) return -1;
            ExperienceMode xp = current.m_XPMode;
            if (xp == null) return -1;
            return (int)xp.m_BaseResourceAvailability;
        }


        private bool IsBunkerScene(string sceneName)
            => !string.IsNullOrEmpty(sceneName) && mManager.SandboxConfigManager.BunkerSceneNames.Contains(sceneName);


        private bool ShouldRemoveBunkerItem()
        {
            double roll = new System.Random().NextDouble();
            double removeChance = 1.0 - (mManager.Settings.InterloperBannedSpawnChance * 0.01);
            return roll < removeChance;
        }
    }
}
