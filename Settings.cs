using ModSettings;
using Il2Cpp;


namespace Trespasser
{
    internal class Settings : JsonModSettings
    {
        internal static Settings Instance;

        [Name("Interloper Item Spawn Chance")]
        [Slider(0, 100)]
        [Description("Chance for items banned on interloper (firearms, hatches, etc) to spawn. Chance is rolled per possible item spawn, and many random spawners have their number per scene reduced compared to Stalker while still maintaining a minimum of one roll per random spawner. Default: 10%")]
        public int InterloperBannedSpawnChance = 10;

        public Settings() : base("Trespasser")
        {
            Initialize();
        }

        protected void Initialize()
        {
            Instance = this;
            AddToModSettings("Trespasser");
            RefreshGUI();
        }
    }
}