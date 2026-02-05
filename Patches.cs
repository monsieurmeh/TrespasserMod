using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using Il2CppTLD.Gameplay;
using MehToolBox;
using MelonLoader;
using UnityEngine;

namespace Trespasser
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.Initialize))]
        internal static class PanelSelectExperience_AddTrespasserMode
        {
            public static void Prefix(ref Panel_SelectExperience __instance)
            {
                SandboxConfig trespasserConfig = SandboxConfigManager.TrespasserConfig;

                GameObject trespasserDisplay = UnityEngine.Object.Instantiate(__instance.m_MenuItems[2].m_Display);
                ConfigureDisplay(trespasserDisplay, __instance.m_MenuItems[2].m_Display, __instance.m_MenuItems[3].m_Display);

                // figure out on-select action to properly activate animation sequence like other difficulties do

                __instance.m_MenuItems.Insert(3, new Panel_SelectExperience.XPModeMenuItem()
                {
                    m_Display = trespasserDisplay,
                    m_SandboxConfig = trespasserConfig
                });
            }


            internal static void ConfigureDisplay(GameObject trespasser, GameObject stalker, GameObject interloper)
            {
                ConfigureDisplayGeneral(trespasser,stalker,interloper);
                ConfigureDisplayText(trespasser, stalker, interloper);
            }


            internal static void ConfigureDisplayGeneral(GameObject trespasser, GameObject stalker, GameObject interloper)
            {
                trespasser.name = "TrespasserDifficultyDisplay";
                trespasser.hideFlags = HideFlags.HideAndDontSave;
            }


            private static void ConfigureDisplayText(GameObject trespasser, GameObject stalker, GameObject interloper)
            {

            }
        }

        // needed to provide runtime gamemodeconfig for savegames when trying to display for loading from main menu
        [HarmonyPatch(typeof(ExperienceModeManager), nameof(ExperienceModeManager.GetGameModeFromName))]
        internal static class ExperienceModeManager_ProvideTrespasserGameModeOnGetGameModeFromName
        {
            internal static void Postfix(string gameModeName, ref GameModeConfig __result)
            {
                MelonLogger.Msg($"GameModeName: {gameModeName}");
                if (gameModeName.Contains("Trespasser"))
                {
                    __result = SandboxConfigManager.TrespasserConfig;
                }
            }
        }



        [HarmonyPatch(typeof(ExperienceModeManager), nameof(ExperienceModeManager.SetGameModeConfig))]
        internal static class ExperienceModeManager_SetGameModeConfigDebug
        {
            internal static bool Prefix(GameModeConfig gameModeConfig, ExperienceModeManager __instance)
            {
                return gameModeConfig != null
                    || ExperienceModeManager.s_CurrentGameMode == null
                    || !ExperienceModeManager.s_CurrentGameMode.m_ModeName.m_LocalizationID.Contains("Trespasser");
            }
        }
    }
}
