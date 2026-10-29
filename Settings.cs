using ModSettings;
using Il2Cpp;


namespace Trespasser
{
    internal class Settings : JsonModSettings
    {
        [Name("Interloper Item Spawn Chance")]
        [Slider(0, 100)]
        [Description("Chance for items banned on interloper (firearms, hatches, etc) to spawn. Chance is rolled per possible item spawn, and many random spawners have their number per scene reduced compared to Stalker while still maintaining a minimum of one roll per random spawner. Default: 10%")]
        public int InterloperBannedSpawnChance = 10;

        [Name("Prepper Cache Loot Rate")]
        [Slider(0, 100)]
        [Description("Sets the amount of loot found in prepper caches compared to Stalker. An interloper-banned item in a prepper cache rolls only the lower of this and its banned-item chance. Default: 10%")]
        public int PrepperCacheLootRate = 10;

        public Settings() : base("Trespasser")
        {
            Initialize();
        }

        protected void Initialize()
        {
            AddToModSettings("Trespasser");
            RefreshGUI();
        }
    }
}