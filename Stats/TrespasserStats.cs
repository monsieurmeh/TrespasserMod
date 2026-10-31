using System.Text.Json;
using Il2Cpp;
using Il2CppTLD.Stats;
using MelonLoader;

namespace Trespasser
{
    internal class TrespasserStats
    {
        private const string FileName = "AllTimeStats.json";

        private class StoredStats
        {
            public int Version { get; set; } = 1;
            public Dictionary<string, float> AllTimeBest { get; set; } = new();
        }

        private TrespasserManager mManager;
        private Dictionary<StatID, float> mAllTimeBest = new();
        private bool mIsLoaded;
        private bool mHasUnsavedChanges;

        public TrespasserStats(TrespasserManager manager)
        {
            mManager = manager;
        }


        public bool TryGetAllTimeBest(StatID id, out float value)
        {
            EnsureLoaded();
            return mAllTimeBest.TryGetValue(id, out value);
        }


        public void Record(StatID id)
        {
            if (id == StatID.Invalid || !mManager.SandboxConfigManager.IsTrespasserMode()) return;
            EnsureLoaded();
            float sessionValue = StatsManager.GetValue(id);
            if (mAllTimeBest.TryGetValue(id, out float best) && best >= sessionValue) return;
            mAllTimeBest[id] = sessionValue;
            mHasUnsavedChanges = true;
        }


        public void Reset()
        {
            mAllTimeBest.Clear();
            Save();
        }


        public void SaveIfChanged()
        {
            if (mHasUnsavedChanges)
                Save();
        }


        public void Load()
        {
            mIsLoaded = true;
            mAllTimeBest.Clear();
            string? json = TrespasserDataFolder.ReadText(FileName);
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                StoredStats? stored = JsonSerializer.Deserialize<StoredStats>(json);
                foreach (KeyValuePair<string, float> entry in stored?.AllTimeBest ?? new())
                {
                    if (Enum.TryParse(entry.Key, out StatID id) && id != StatID.Invalid)
                        mAllTimeBest[id] = entry.Value;
                }
            }
            catch (JsonException exception)
            {
                MelonLogger.Warning($"Ignoring unreadable {FileName}: {exception.Message}");
            }
        }


        private void Save()
        {
            StoredStats stored = new();
            foreach (KeyValuePair<StatID, float> entry in mAllTimeBest)
                stored.AllTimeBest[entry.Key.ToString()] = entry.Value;
            TrespasserDataFolder.WriteText(FileName, JsonSerializer.Serialize(stored));
            mHasUnsavedChanges = false;
        }


        private void EnsureLoaded()
        {
            if (!mIsLoaded)
                Load();
        }
    }
}
