using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace Trespasser
{
    internal class LootManager
    {
        private TrespasserManager mManager;
        private bool mIsSceneRestored;
        private HashSet<int> mBunkerDecidedItems = new();

        public LootManager(TrespasserManager manager)
        {
            mManager = manager;
        }

        private double BannedItemChance => mManager.Settings.InterloperBannedSpawnChance * 0.01;
        private double BunkerLootChance => mManager.Settings.PrepperCacheLootRate * 0.01;


        public void OnLoadSceneData(bool isSceneRestored)
        {
            mIsSceneRestored = isSceneRestored;
            mBunkerDecidedItems.Clear();
        }


        public bool ShouldAllowBannedItem(Component instance)
        {
            if (instance == null) return false;
            if (mIsSceneRestored) return false;
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return false;
            if (instance.GetComponent<GearItem>() == null) return false;

            bool inBunker = IsBunkerScene(instance.gameObject.scene.name);
            double chance = inBunker ? Math.Min(BannedItemChance, BunkerLootChance) : BannedItemChance;
            bool shouldAllow = new System.Random().NextDouble() <= chance;
            if (shouldAllow && inBunker)
                mBunkerDecidedItems.Add(instance.gameObject.GetInstanceID());
            return shouldAllow;
        }


        public void MaybeRemoveFromBunker(GearItem gearItem)
        {
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return;
            if (mIsSceneRestored) return;
            if (!IsBunkerScene(gearItem.gameObject.scene.name)) return;
            if (!gearItem.gameObject.activeSelf) return;
            if (mBunkerDecidedItems.Remove(gearItem.gameObject.GetInstanceID())) return;
            if (!ShouldRemoveBunkerItem()) return;

            gearItem.gameObject.SetActive(false);
        }


        public void MaybeThinBunkerContents(Container container)
        {
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return;
            if (mIsSceneRestored) return;

            string sceneName = container.gameObject.scene.name;
            if (!IsBunkerScene(sceneName)) return;

            Il2CppSystem.Collections.Generic.List<string> gearList = container.m_GearToInstantiate;
            if (gearList == null || gearList.Count == 0) return;

            for (int i = gearList.Count - 1; i >= 0; i--)
            {
                if (ShouldRemoveBunkerItem())
                    gearList.RemoveAt(i);
            }
        }


        public void FixInterloperQuantity(RandomSpawnObject spawner)
        {
            if (!mManager.SandboxConfigManager.IsTrespasserMode()) return;

            if (spawner.m_NumObjectsToEnableByLevel == null || spawner.m_NumObjectsToEnableByLevel.Length < 2) return;
            int currentIndex = GetCurrentResourceIndex();
            if (currentIndex < 0 || currentIndex >= spawner.m_NumObjectsToEnableByLevel.Length) return;
            if (spawner.m_NumObjectsToEnableByLevel[currentIndex] != 0) return;
            if (spawner.m_NumObjectsToEnableByLevel[currentIndex + 1] == 0) return;
            if (!HasLootComponent(spawner)) return;
            spawner.m_NumObjectsToEnableByLevel[currentIndex] = Math.Max(1, (int)(spawner.m_NumObjectsToEnableByLevel[currentIndex + 1] * 0.5));
        }


        private bool HasLootComponent(RandomSpawnObject spawner)
        {
            Il2CppReferenceArray<GameObject> objectList = spawner.m_ObjectList;
            if (objectList == null) return false;

            for (int i = 0; i < objectList.Length; i++)
            {
                GameObject go = objectList[i];
                if (go == null) continue;
                if (go.GetComponentInChildren<GearItem>(true) != null) return true;
                if (go.GetComponentInChildren<BodyHarvest>(true) != null) return true;
            }

            return false;
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
            double removeChance = 1.0 - BunkerLootChance;
            return roll < removeChance;
        }
    }
}
