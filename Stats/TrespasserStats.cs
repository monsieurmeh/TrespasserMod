using System.Text.Json;
using Il2Cpp;
using Il2CppTLD.Stats;
using MelonLoader;
using ModData;

namespace Trespasser
{
    internal class TrespasserStats
    {
        private const string ProfileSuffix = "AllTimeStats";

        private class StoredStats
        {
            public int Version { get; set; } = 1;
            public Dictionary<string, float> AllTimeBest { get; set; } = new();
        }

        private class ProfileStore
        {
            private ModDataManager mModData = new("Trespasser", false);

            public string? Read() => mModData.LoadFromProfile(ProfileSuffix);

            public bool Write(string text) => mModData.SaveToProfile(text, ProfileSuffix);
        }

        private TrespasserManager mManager;
        private ProfileStore? mProfile;
        private Dictionary<StatID, float> mAllTimeBest = new();
        private object mLock = new();
        private bool mIsLoaded;
        private bool mHasUnsavedChanges;
        private bool mProfileUnavailable;

        public TrespasserStats(TrespasserManager manager)
        {
            mManager = manager;
        }


        public bool TryGetAllTimeBest(StatID id, out float value)
        {
            lock (mLock)
            {
                EnsureLoaded();
                return mAllTimeBest.TryGetValue(id, out value);
            }
        }


        public void Record(StatID id)
        {
            if (id == StatID.Invalid || !mManager.SandboxConfigManager.IsTrespasserMode()) return;
            float sessionValue = StatsManager.GetValue(id);
            lock (mLock)
            {
                EnsureLoaded();
                if (mAllTimeBest.TryGetValue(id, out float best) && best >= sessionValue) return;
                mAllTimeBest[id] = sessionValue;
                mHasUnsavedChanges = true;
            }
        }


        public void Reset()
        {
            lock (mLock)
            {
                mAllTimeBest.Clear();
                Save();
            }
        }


        public void SaveIfChanged()
        {
            lock (mLock)
            {
                if (mHasUnsavedChanges)
                    Save();
            }
        }


        public void Load()
        {
            lock (mLock)
            {
                mIsLoaded = true;
                mAllTimeBest.Clear();
                string? json = ReadProfile();
                if (json != null)
                    Read(json, "the profile");
            }
        }


        private void Read(string json, string source)
        {
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
                MelonLogger.Warning($"Ignoring unreadable all-time Trespasser stats in {source}: {exception.Message}");
            }
        }


        private void Save()
        {
            StoredStats stored = new();
            foreach (KeyValuePair<StatID, float> entry in mAllTimeBest)
                stored.AllTimeBest[entry.Key.ToString()] = entry.Value;
            mHasUnsavedChanges = !WriteProfile(JsonSerializer.Serialize(stored));
        }


        private string? ReadProfile()
        {
            if (mProfileUnavailable) return null;
            try
            {
                return Profile().Read();
            }
            catch (MissingMethodException)
            {
                ProfileMissing();
                return null;
            }
        }


        private bool WriteProfile(string json)
        {
            if (mProfileUnavailable) return false;
            try
            {
                return Profile().Write(json);
            }
            catch (MissingMethodException)
            {
                ProfileMissing();
                return false;
            }
        }


        private ProfileStore Profile() => mProfile ??= new ProfileStore();


        private void ProfileMissing()
        {
            mProfileUnavailable = true;
            MelonLogger.Warning("All-time Trespasser stats need ModData 1.6.0 or newer; they won't be kept until ModData is updated");
        }


        private void EnsureLoaded()
        {
            if (!mIsLoaded)
                Load();
        }
    }
}
