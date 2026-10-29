using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;

namespace Trespasser
{
    internal static class SandboxPatches
    {
        [HarmonyPatch(typeof(ExperienceModeManager), nameof(ExperienceModeManager.GetGameModeFromName))]
        internal static class ExperienceModeManager_ProvideTrespasserGameModeOnGetGameModeFromName
        {
            internal static void Postfix(string gameModeName, ref GameModeConfig __result) => TrespasserManager.Instance.SandboxConfigManager.ProvideTrespasserGameMode(gameModeName, ref __result);
        }


        [HarmonyPatch(typeof(ExperienceModeManager), nameof(ExperienceModeManager.SetGameModeConfig))]
        internal static class ExperienceModeManager_SetGameModeConfigDebug
        {
            internal static bool Prefix(GameModeConfig gameModeConfig) => TrespasserManager.Instance.SandboxConfigManager.ShouldSetGameModeConfig(gameModeConfig);
        }
    }
}
