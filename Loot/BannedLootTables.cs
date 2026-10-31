using Il2Cpp;
using Il2CppTLD.Gameplay;
using UnityEngine;

namespace Trespasser
{
    internal class BannedLootTables
    {
        private class TableBans
        {
            public List<(GameObject prefab, int weight)> Entries = new();
            public int TotalWeight;
        }

        private LootManager mLootManager;
        private Dictionary<IntPtr, TableBans> mTableBans = new();

        public BannedLootTables(LootManager lootManager)
        {
            mLootManager = lootManager;
        }


        public void Clear() => mTableBans.Clear();


        public GameObject MaybeDrawBanned(LootTableData table, LootSource source, string sceneName)
        {
            if (table == null) return null;

            TableBans bans = GetTableBans(table);
            if (bans.TotalWeight == 0) return null;

            int allowedWeight = table.m_FilteredExtendedItems?.Count ?? 0;
            double drawChance = mLootManager.BannedDrawChance(mLootManager.ChanceFor(source, sceneName), bans.TotalWeight, allowedWeight);
            return mLootManager.Roll(drawChance) ? PickWeighted(bans) : null;
        }


        private TableBans GetTableBans(LootTableData table)
        {
            if (mTableBans.TryGetValue(table.Pointer, out TableBans cached) && !cached.Entries.Any(entry => entry.prefab == null))
                return cached;

            TableBans bans = BuildTableBans(table);
            mTableBans[table.Pointer] = bans;
            return bans;
        }


        private TableBans BuildTableBans(LootTableData table)
        {
            TableBans bans = new();
            table.MaybeInitializeTable();

            Il2CppSystem.Collections.Generic.List<RandomTableDataEntry<AssetReferenceGearItem>> entries = table.m_BaseEntries;
            if (entries == null) return bans;

            for (int i = 0; i < entries.Count; i++)
            {
                RandomTableDataEntry<AssetReferenceGearItem> entry = entries[i];
                if (entry?.m_Item == null || entry.m_Weight <= 0) continue;
                if (!table.ShouldFilterOutItem(entry.m_Item)) continue;
                if (mLootManager.WithBansLifted(() => table.ShouldFilterOutItem(entry.m_Item))) continue;

                GameObject prefab = table.LoadPrefab(entry.m_Item);
                if (prefab == null) continue;
                bans.Entries.Add((prefab, entry.m_Weight));
                bans.TotalWeight += entry.m_Weight;
            }
            return bans;
        }


        private GameObject PickWeighted(TableBans bans)
        {
            int pick = new System.Random().Next(bans.TotalWeight);
            foreach ((GameObject prefab, int weight) in bans.Entries)
            {
                if (pick < weight) return prefab;
                pick -= weight;
            }
            return null;
        }
    }
}
