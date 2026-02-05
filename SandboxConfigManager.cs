using Il2Cpp;
using Il2CppTLD.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trespasser
{
    internal class SandboxConfigManager
    {
        private static SandboxConfig _TrespasserConfig;

        public static SandboxConfig TrespasserConfig
        {
            get
            {
                if (_TrespasserConfig == null)
                {
                    _TrespasserConfig = InstantiateSandbox();
                }
                return _TrespasserConfig;
            }
            set
            {
                _TrespasserConfig = value;
            }
        }

        private static SandboxConfig InstantiateSandbox()
        {
            SandboxConfig trespasserConfig = new SandboxConfig();
            SandboxConfig stalkerConfig = ExperienceModeManager.GetGameModeFromName("Stalker").Cast<SandboxConfig>();
            SandboxConfig interloperConfig = ExperienceModeManager.GetGameModeFromName("Interloper").Cast<SandboxConfig>();
            ConfigureSandbox(trespasserConfig, stalkerConfig, interloperConfig);
            return trespasserConfig;
        }


        private static void ConfigureSandbox(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            ConfigureSandboxGeneral(trespasser, stalker, interloper);
            ConfigureSandboxBunkerSetup(trespasser, stalker, interloper);
            ConfigureSandboxXPMode(trespasser, stalker, interloper);
            ConfigureSandboxAvailableRegions(trespasser, stalker, interloper);
            ConfigureSandboxSceneLoadConditions(trespasser, stalker, interloper);

            MehToolBox.ScriptExaminer.Compare(trespasser, stalker);
        }

        private static void ConfigureSandboxGeneral(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.name = "Trespasser";
            trespasser.m_NumFeats = 2;
            trespasser.m_ForceSpawnPoint = string.Empty;
            trespasser.m_MissionServicesPrefab = stalker.m_MissionServicesPrefab;
            trespasser.m_ModeName = new LocalizedString() { m_LocalizationID = "Trespasser" }; //should be GAMEPLAY_Trespasser
            trespasser.m_Description = new LocalizedString() { m_LocalizationID = "Trespasser Description" };
            trespasser.m_LoadingText = new LocalizedString() { m_LocalizationID = "Trespasser Loading Text" };
            trespasser.m_SpriteName = stalker.m_SpriteName;
            trespasser.m_ActiveTags = stalker.m_ActiveTags;
            trespasser.m_SaveSlotType = SaveSlotType.SANDBOX;
        }

        private static void ConfigureSandboxBunkerSetup(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_BunkerSetup = UnityEngine.Object.Instantiate(stalker.m_BunkerSetup);
        }


        private static void ConfigureSandboxXPMode(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode = UnityEngine.Object.Instantiate(stalker.m_XPMode);
            ConfigureSandboxStartGear(trespasser, stalker, interloper);
            ConfigureSandboxCougarSettings(trespasser, stalker, interloper);
        }

        private static void ConfigureSandboxStartGear(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode.m_StartGear = UnityEngine.Object.Instantiate(stalker.m_XPMode.m_StartGear);
        }


        private static void ConfigureSandboxCougarSettings(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode.m_CougarSettings = UnityEngine.Object.Instantiate(interloper.m_XPMode.m_CougarSettings);
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
    }
}
