using MelonLoader;
using MelonLoader.Utils;

namespace Trespasser
{
    internal static class TrespasserDataFolder
    {
        private static readonly string mFolderPath = Path.Combine(MelonEnvironment.ModsDirectory, "Trespasser");


        public static string? ReadText(string fileName)
        {
            string path = Path.Combine(mFolderPath, fileName);
            try
            {
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch (Exception exception)
            {
                MelonLogger.Warning($"Could not read {path}: {exception.Message}");
                return null;
            }
        }


        public static void WriteText(string fileName, string contents)
        {
            string path = Path.Combine(mFolderPath, fileName);
            try
            {
                Directory.CreateDirectory(mFolderPath);
                File.WriteAllText(path + ".tmp", contents);
                File.Move(path + ".tmp", path, true);
            }
            catch (Exception exception)
            {
                MelonLogger.Warning($"Could not write {path}: {exception.Message}");
            }
        }
    }
}
