using System.Text.Json;
using MelonLoader;

namespace Trespasser
{
    internal class BanExemptions
    {
        private const string FileName = "BanExemptions.json";

        private static readonly string[] mDefaultItems =
        {
            "GEAR_Rifle_Barbs", "GEAR_Rifle_Curators", "GEAR_Rifle_Trader", "GEAR_Rifle_Vaughns",
            "GEAR_RevolverFancy", "GEAR_RevolverGreen", "GEAR_RevolverStubNosed",
            "GEAR_Bow_Bushcraft", "GEAR_Bow_Manufactured", "GEAR_Bow_Woodwrights",
        };

        private class StoredExemptions
        {
            public int Version { get; set; } = 1;
            public List<string> Items { get; set; } = new();
        }

        private TrespasserManager mManager;
        private HashSet<string>? mExemptItems;

        public BanExemptions(TrespasserManager manager)
        {
            mManager = manager;
        }


        public bool IsExempt(string itemName)
        {
            if (!mManager.Settings.EnsureCollectablesSpawn || string.IsNullOrEmpty(itemName)) return false;
            mExemptItems ??= Load();
            return mExemptItems.Contains(itemName.Replace("(Clone)", string.Empty).Trim());
        }


        private HashSet<string> Load()
        {
            HashSet<string> items = new(StringComparer.OrdinalIgnoreCase);
            string? json = TrespasserDataFolder.ReadText(FileName);
            if (json == null)
            {
                items.UnionWith(mDefaultItems);
                StoredExemptions defaults = new() { Items = new List<string>(mDefaultItems) };
                TrespasserDataFolder.WriteText(FileName, JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true }));
                return items;
            }

            try
            {
                StoredExemptions stored = JsonSerializer.Deserialize<StoredExemptions>(json) ?? new();
                items.UnionWith(stored.Items.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()));
            }
            catch (JsonException exception)
            {
                MelonLogger.Warning($"{FileName} is not valid JSON ({exception.Message}); using the default exemptions");
                items.UnionWith(mDefaultItems);
            }
            return items;
        }
    }
}
