using ModSettings;
using Il2Cpp;


namespace Trespasser
{
    internal class Settings : JsonModSettings
    {
        public Settings() : base(Path.Combine("Trespasser", "Trespasser"))
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