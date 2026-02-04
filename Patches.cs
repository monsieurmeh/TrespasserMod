using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gameplay;
using MehToolBox;
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
                SandboxConfig trespasserConfig = new SandboxConfig();
                ConfigureSandbox(trespasserConfig, __instance.m_MenuItems[2].m_SandboxConfig, __instance.m_MenuItems[3].m_SandboxConfig);

                GameObject trespasserDisplay = UnityEngine.Object.Instantiate(__instance.m_MenuItems[2].m_Display);
                ConfigureDisplay(trespasserDisplay, __instance.m_MenuItems[2].m_Display, __instance.m_MenuItems[3].m_Display);

                // figure out on-select action to properly activate animation sequence like other difficulties do

                __instance.m_MenuItems.Insert(2, new Panel_SelectExperience.XPModeMenuItem()
                {
                    m_Display = trespasserDisplay,
                    m_SandboxConfig = trespasserConfig
                });
            }

            #region sandbox

            private static void ConfigureSandbox(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                ConfigureSandboxGeneral(trespasser, stalker, interloper);
                ConfigureSandboxBunkerSetup(trespasser, stalker, interloper);
                ConfigureSandboxXPMode(trespasser, stalker, interloper);
                ConfigureSandboxStartGear(trespasser, stalker, interloper);
                ConfigureSandboxAvailableRegions(trespasser, stalker, interloper);
                ConfigureSandboxSceneLoadConditions(trespasser, stalker, interloper);

            }

            private static void ConfigureSandboxGeneral(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                trespasser.m_NumFeats = 2;
                trespasser.m_ForceSpawnPoint = string.Empty;
                trespasser.m_MissionServicesPrefab = stalker.m_MissionServicesPrefab;
                trespasser.m_ModeName = new LocalizedString() { m_LocalizationID = "Trespasser" }; //should be GAMEPLAY_Trespasser
                trespasser.m_Description = new LocalizedString() { m_LocalizationID = "Trespasser Description" };
                trespasser.m_LoadingText = new LocalizedString() { m_LocalizationID = "Trespasser Loading Text" };
                trespasser.m_SpriteName = stalker.m_SpriteName;
            }

            private static void ConfigureSandboxBunkerSetup(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                trespasser.m_BunkerSetup = UnityEngine.Object.Instantiate(stalker.m_BunkerSetup);
            }


            private static void ConfigureSandboxXPMode(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                trespasser.m_XPMode = UnityEngine.Object.Instantiate(stalker.m_XPMode);
            }


            private static void ConfigureSandboxStartGear(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                trespasser.m_XPMode.m_StartGear = UnityEngine.Object.Instantiate(stalker.m_XPMode.m_StartGear);
            }


            private static void ConfigureSandboxAvailableRegions(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                trespasser.m_AvailableStartRegions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppTLD.Scenes.RegionSpecification>(interloper.m_AvailableStartRegions.Length);
                for (int i = 0, iMax = interloper.m_AvailableStartRegions.Length; i < iMax; i++)
                {
                    trespasser.m_AvailableStartRegions[i] = UnityEngine.Object.Instantiate(interloper.m_AvailableStartRegions[i]);
                }
            }

            private static void ConfigureSandboxSceneLoadConditions(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
            {
                trespasser.m_LoadConditions = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppTLD.Scenes.SceneLoadCondition>(interloper.m_LoadConditions.Length);
                for (int i = 0, iMax = interloper.m_LoadConditions.Length; i < iMax; i++)
                {
                    trespasser.m_LoadConditions[i] = UnityEngine.Object.Instantiate(interloper.m_LoadConditions[i]);
                }
            }

            #endregion


            #region display

            private static void ConfigureDisplay(GameObject trespasser, GameObject stalker, GameObject interloper)
            {
                ConfigureDisplayGeneral(trespasser,stalker,interloper);
                ConfigureDisplayText(trespasser, stalker, interloper);
            }


            private static void ConfigureDisplayGeneral(GameObject trespasser, GameObject stalker, GameObject interloper)
            {
                trespasser.name = "TrespasserDifficultyDisplay";
                trespasser.hideFlags = HideFlags.HideAndDontSave;
            }


            private static void ConfigureDisplayText(GameObject trespasser, GameObject stalker, GameObject interloper)
            {

            }
 

            #endregion
        }

    }
}
