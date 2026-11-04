using System.Reflection;
using System.Text.Json;
using MelonLoader;

namespace Trespasser
{
    internal class BanExemptions
    {
        private const string FileName = "BanExemptions.json";

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
                json = DefaultJson();
                TrespasserDataFolder.WriteText(FileName, json);
            }

            try
            {
                items.UnionWith(Parse(json));
            }
            catch (JsonException exception)
            {
                MelonLogger.Warning($"{FileName} is not valid JSON ({exception.Message}); using the default exemptions");
                items.UnionWith(Parse(DefaultJson()));
            }
            return items;
        }


        private static IEnumerable<string> Parse(string json)
        {
            StoredExemptions stored = JsonSerializer.Deserialize<StoredExemptions>(json) ?? new();
            return stored.Items.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim());
        }


        private static string DefaultJson()
        {
            Assembly assembly = typeof(BanExemptions).Assembly;
            string resourceName = assembly.GetManifestResourceNames().First(name => name.EndsWith(FileName, StringComparison.OrdinalIgnoreCase));
            using StreamReader reader = new(assembly.GetManifestResourceStream(resourceName));
            return reader.ReadToEnd();
        }
    }
}
