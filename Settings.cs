using ModSettings;
using Il2Cpp;


namespace Trespasser
{
    internal class Settings : JsonModSettings
    {
        [Section("Interloper-banned items")]
        [Name("World Spawn Chance")]
        [Slider(0, 100)]
        [Description("Chance for each interloper-banned item (firearms, hatchets, etc) placed in the world to spawn. Many random spawners have their number per scene reduced compared to Stalker while still keeping at least one roll per spawner. Default: 10%")]
        public int InterloperBannedSpawnChance = 10;

        [Name("Container Chance")]
        [Slider(0, 100)]
        [Description("Chance for each container loot roll that would have given an interloper-banned item on Stalker to keep it. Otherwise the roll gives what Interloper would. Default: 10%")]
        public int ContainerBannedChance = 10;

        [Name("Beachcombing Chance")]
        [Slider(0, 100)]
        [Description("Chance for each beachcombing item roll that would have given an interloper-banned item on Stalker to keep it. Otherwise the roll gives what Interloper would. Default: 10%")]
        public int BeachcombingBannedChance = 10;

        [Name("Trader Chance")]
        [Slider(0, 100)]
        [Description("Chance for each trade the trader rolls that would have offered or asked for an interloper-banned item on Stalker to keep it. Otherwise the trader rolls what Interloper would. Default: 10%")]
        public int TraderBannedChance = 10;

        [Section("Loot thinning")]
        [Name("Prepper Cache Loot Rate")]
        [Slider(0, 100)]
        [Description("Sets the amount of loot found in prepper caches compared to Stalker. Chances never stack: an interloper-banned item in a prepper cache rolls only the lower of this and its banned-item chance. Default: 10%")]
        public int PrepperCacheLootRate = 10;

        [Name("Random Spawner Quantity")]
        [Slider(0, 100)]
        [Description("For random loot spawners that spawn nothing on Interloper, how many items they spawn compared to Stalker (always at least one). Default: 50%")]
        public int RandomSpawnerQuantity = 50;

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