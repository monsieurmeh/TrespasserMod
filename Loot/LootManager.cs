using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Trader;
using UnityEngine;

namespace Trespasser
{
    internal enum LootSource
    {
        World,
        Container,
        Beachcombing,
        Trader,
    }


    internal class LootManager
    {
        private TrespasserManager mManager;
        private BannedLootTables mBannedLootTables;
        private BannedTrades mBannedTrades;
        private BanExemptions mBanExemptions;
        private InterloperBans mInterloperBans = new();
        private bool mIsSceneRestored;
        private HashSet<int> mBunkerDecidedItems = new();
        private List<string> mRolledContainerPrefabs = new();
        private LootSource? mPrefabRollSource;
        private string mPrefabRollScene = string.Empty;
        private int mBansLifted;

        public LootManager(TrespasserManager manager)
        {
            mManager = manager;
            mBannedLootTables = new BannedLootTables(this);
            mBannedTrades = new BannedTrades(this);
            mBanExemptions = new BanExemptions(manager);
        }

        private double BunkerLootChance => mManager.Settings.PrepperCacheLootRate * 0.01;


        public void OnLoadSceneData(bool isSceneRestored)
        {
            mIsSceneRestored = isSceneRestored;
            mBunkerDecidedItems.Clear();
            mBannedLootTables.Clear();
        }


        public bool ShouldAllowBannedItem(Component instance)
        {
            if (instance == null) return false;
            if (!IsTrespasserLootActive()) return false;
            if (!mInterloperBans.IsInterloperBan(instance)) return false;
            if (IsSwitchedOffInCustomGame(instance)) return false;
            if (mBansLifted > 0 || mBanExemptions.IsExempt(instance.gameObject.name)) return true;

            GearItem gearItem = instance.GetComponent<GearItem>();
            if (gearItem == null) return false;
            if (!instance.gameObject.scene.IsValid()) return RollPrefab(instance.gameObject);
            if (WasAlreadyDecided(gearItem)) return true;
            if (mIsSceneRestored) return false;

            string sceneName = instance.gameObject.scene.name;
            bool shouldAllow = Roll(ChanceFor(LootSource.World, sceneName));
            if (shouldAllow && IsBunkerScene(sceneName))
                mBunkerDecidedItems.Add(instance.gameObject.GetInstanceID());
            return shouldAllow;
        }


        public void LiftBans() => mBansLifted++;


        public void RestoreBans() => mBansLifted = Math.Max(0, mBansLifted - 1);


        public T WithBansLifted<T>(Func<T> query)
        {
            LiftBans();
            try
            {
                return query();
            }
            finally
            {
                RestoreBans();
            }
        }


        public void BeginPrefabRolls(LootSource source, Component instance)
        {
            mPrefabRollSource = source;
            mPrefabRollScene = instance.gameObject.scene.name ?? string.Empty;
        }


        public void EndPrefabRolls()
        {
            mPrefabRollSource = null;
            mPrefabRollScene = string.Empty;
        }


        public void BeginContainerPopulation() => mRolledContainerPrefabs.Clear();


        public void RollRandomContainerGear(Container container, int firstNewIndex)
        {
            if (!IsTrespasserLootActive()) return;

            Il2CppSystem.Collections.Generic.List<string> gearList = container.m_GearToInstantiate;
            if (gearList == null) return;

            LootTableData table = container.m_LootTable ?? container.m_LootTableData;
            string sceneName = container.gameObject.scene.name;
            for (int i = firstNewIndex; i < gearList.Count; i++)
            {
                GameObject banned = mBannedLootTables.MaybeDrawBanned(table, LootSource.Container, sceneName);
                if (banned == null) continue;
                gearList[i] = banned.name;
                mRolledContainerPrefabs.Add(banned.name);
            }
        }


        public void RollRadialDraw(RadialObjectSpawner spawner, ref GameObject prefab)
        {
            if (!IsTrespasserLootActive()) return;

            LootSource source = IsBeachcombing(spawner) ? LootSource.Beachcombing : LootSource.World;
            GameObject banned = mBannedLootTables.MaybeDrawBanned(spawner.ActiveLootTableData, source, spawner.gameObject.scene.name);
            if (banned != null)
                prefab = banned;
        }


        public void KeepRolledRadialSpawn(RadialObjectSpawner spawner, GameObject prefab, GameObject spawned)
        {
            if (spawned == null || spawner.ActiveLootTableData == null) return;
            if (!IsTrespasserLootActive()) return;
            if (!DisableForCurrentModeExtensions.ShouldDisableForCurrentMode(prefab)) return;

            foreach (DisableObjectForXPMode disabler in spawned.GetComponents<DisableObjectForXPMode>())
                disabler.enabled = false;
            foreach (DisableObjectForGameMode disabler in spawned.GetComponents<DisableObjectForGameMode>())
                disabler.enabled = false;
        }


        public void RollTrade(TradeExchange exchange, TraderState traderState, ref TradeExchangeData draw)
        {
            if (!IsTrespasserLootActive()) return;
            draw = mBannedTrades.MaybeDrawBanned(exchange, traderState, draw);
        }


        public void OnExchangeFiltered(TradeExchange exchange) => mBannedTrades.Forget(exchange);


        public void MaybeRemoveFromBunker(GearItem gearItem)
        {
            if (!IsTrespasserLootActive()) return;
            if (mIsSceneRestored) return;
            if (!IsBunkerScene(gearItem.gameObject.scene.name)) return;
            if (!gearItem.gameObject.activeSelf) return;
            if (mBunkerDecidedItems.Remove(gearItem.gameObject.GetInstanceID())) return;
            if (mBanExemptions.IsExempt(gearItem.gameObject.name)) return;
            if (!ShouldRemoveBunkerItem()) return;

            gearItem.gameObject.SetActive(false);
        }


        public void MaybeThinBunkerContents(Container container)
        {
            if (!IsTrespasserLootActive()) return;

            string sceneName = container.gameObject.scene.name;
            if (!IsBunkerScene(sceneName)) return;

            Il2CppSystem.Collections.Generic.List<string> gearList = container.m_GearToInstantiate;
            if (gearList == null || gearList.Count == 0) return;

            for (int i = gearList.Count - 1; i >= 0; i--)
            {
                if (mRolledContainerPrefabs.Remove(gearList[i]) || mBanExemptions.IsExempt(gearList[i])) continue;
                if (ShouldRemoveBunkerItem())
                    gearList.RemoveAt(i);
            }
        }


        public void FixInterloperQuantity(RandomSpawnObject spawner)
        {
            if (!IsTrespasserLootActive()) return;

            if (spawner.m_NumObjectsToEnableByLevel == null || spawner.m_NumObjectsToEnableByLevel.Length < 2) return;
            int currentIndex = GetCurrentResourceIndex();
            if (currentIndex < 0 || currentIndex >= spawner.m_NumObjectsToEnableByLevel.Length) return;
            if (spawner.m_NumObjectsToEnableByLevel[currentIndex] != 0) return;
            if (spawner.m_NumObjectsToEnableByLevel[currentIndex + 1] == 0) return;
            if (!HasLootComponent(spawner)) return;
            double quantity = mManager.Settings.RandomSpawnerQuantity * 0.01;
            spawner.m_NumObjectsToEnableByLevel[currentIndex] = Math.Max(1, (int)(spawner.m_NumObjectsToEnableByLevel[currentIndex + 1] * quantity));
        }


        public double ChanceFor(LootSource source, string sceneName)
        {
            double chance = SourceChance(source);
            return IsBunkerScene(sceneName) ? Math.Min(chance, BunkerLootChance) : chance;
        }


        public double BannedDrawChance(double allowChance, int bannedWeight, int allowedWeight)
        {
            if (bannedWeight <= 0 || allowChance <= 0) return 0;
            return allowChance * bannedWeight / (allowedWeight + bannedWeight);
        }


        public bool Roll(double chance) => new System.Random().NextDouble() < chance;


        private bool IsTrespasserLootActive()
            => mManager.SandboxConfigManager.IsTrespasserMode() || mManager.CustomModeManager.IsTrespasserLootActive();


        private bool IsSwitchedOffInCustomGame(Component instance)
        {
            ExperienceModeManager manager = GameManager.GetExperienceModeManagerComponent();
            if (manager == null || !manager.InCustomMode()) return false;
            CustomExperienceMode customMode = manager.GetCustomMode();
            GearItem gearItem = instance.GetComponent<GearItem>();
            if (customMode == null || gearItem == null) return false;
            if (!customMode.m_RiflesInWorld && gearItem.IsGunOrAmmoOrCasingOfType(GunType.Rifle)) return true;
            return !customMode.m_RevolversInWorld && gearItem.IsGunOrAmmoOrCasingOfType(GunType.Revolver);
        }


        private bool RollPrefab(GameObject prefab)
        {
            if (mPrefabRollSource == null) return false;
            bool shouldAllow = Roll(ChanceFor(mPrefabRollSource.Value, mPrefabRollScene));
            if (shouldAllow && mPrefabRollSource == LootSource.Container)
                mRolledContainerPrefabs.Add(prefab.name);
            return shouldAllow;
        }


        private bool WasAlreadyDecided(GearItem gearItem)
            => gearItem.m_BeenInPlayerInventory || gearItem.m_InPlayerInventory || gearItem.m_BeenInContainer || gearItem.m_InsideContainer;


        private double SourceChance(LootSource source)
        {
            Settings settings = mManager.Settings;
            int percent = source switch
            {
                LootSource.Container => settings.ContainerBannedChance,
                LootSource.Beachcombing => settings.BeachcombingBannedChance,
                LootSource.Trader => settings.TraderBannedChance,
                _ => settings.InterloperBannedSpawnChance,
            };
            return percent * 0.01;
        }


        private bool IsBeachcombing(RadialObjectSpawner spawner)
            => spawner.IsTidelineSpawner || spawner.GetComponentInParent<BeachcombingSpawner>() != null;


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
