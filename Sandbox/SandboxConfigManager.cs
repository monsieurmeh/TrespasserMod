using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.AddressableAssets;
using Il2CppTLD.AI;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gear;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Trespasser
{
    internal class SandboxConfigManager
    {
        private SandboxConfig mTrespasserConfig;

        internal HashSet<string> BunkerSceneNames { get; } = new(StringComparer.OrdinalIgnoreCase);

        public SandboxConfig TrespasserConfig
        {
            get
            {
                if (mTrespasserConfig == null)
                    mTrespasserConfig = InstantiateSandbox();
                return mTrespasserConfig;
            }
            set => mTrespasserConfig = value;
        }

        #region Game Mode

        internal bool IsTrespasserMode() => ExperienceModeManager.s_CurrentGameMode != null && ExperienceModeManager.s_CurrentGameMode.m_ModeName.m_LocalizationID.Contains("Trespasser");

        internal bool IsTrespasserSave(SaveSlotInfo slotInfo)
        {
            if (slotInfo == null) return false;
            GameModeConfig config = slotInfo.m_GameModeConfig;
            if (config == null) return false;
            return config.m_ModeName != null
                && config.m_ModeName.m_LocalizationID != null
                && config.m_ModeName.m_LocalizationID.Contains("Trespasser");
        }

        internal void ProvideTrespasserGameMode(string gameModeName, ref GameModeConfig gameModeConfig)
        {
            if (gameModeName.Contains("Trespasser"))
            {
                gameModeConfig = TrespasserConfig;
            }
        }

        internal bool ShouldSetGameModeConfig(GameModeConfig gameModeConfig)
        {
            return gameModeConfig != null
                || ExperienceModeManager.s_CurrentGameMode == null
                || !ExperienceModeManager.s_CurrentGameMode.m_ModeName.m_LocalizationID.Contains("Trespasser");
        }

        #endregion

        #region Top-Level Configuration

        private SandboxConfig InstantiateSandbox()
        {
            var trespasser = new SandboxConfig();
            var stalker = ExperienceModeManager.GetGameModeFromName("Stalker").Cast<SandboxConfig>();
            var interloper = ExperienceModeManager.GetGameModeFromName("Interloper").Cast<SandboxConfig>();

            ConfigureSandbox(trespasser, stalker, interloper);
            return trespasser;
        }

        private void ConfigureSandbox(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            ConfigureSandboxGeneral(trespasser, stalker, interloper);
            ConfigureSandboxBunkerSetup(trespasser, stalker, interloper);
            ConfigureSandboxXPMode(trespasser, stalker, interloper);
            ConfigureSandboxAvailableRegions(trespasser, stalker, interloper);
            ConfigureSandboxSceneLoadConditions(trespasser, stalker, interloper);
        }

        #endregion

        #region General Settings

        private void ConfigureSandboxGeneral(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.name = "Trespasser";
            trespasser.m_NumFeats = 2;
            trespasser.m_ForceSpawnPoint = string.Empty;
            trespasser.m_MissionServicesPrefab = stalker.m_MissionServicesPrefab;
            trespasser.m_ModeName = new LocalizedString() { m_LocalizationID = "Trespasser" };
            trespasser.m_Description = new LocalizedString() { m_LocalizationID = "A stepping stone between Stalker and Interloper. Start with a few matches and find sparse loot around the world that you wouldn't find on Interloper." };
            trespasser.m_LoadingText = new LocalizedString() { m_LocalizationID = "A stepping stone between Stalker and Interloper. Start with a few matches and find sparse loot around the world that you wouldn't find on Interloper." };
            trespasser.m_SpriteName = stalker.m_SpriteName;
            trespasser.m_ActiveTags = interloper.m_ActiveTags;
            trespasser.m_SaveSlotType = SaveSlotType.SANDBOX;

        }

        private void ConfigureSandboxBunkerSetup(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_BunkerSetup = UnityEngine.Object.Instantiate(stalker.m_BunkerSetup);
            CollectBunkerSceneNames(trespasser);
        }

        private void CollectBunkerSceneNames(SandboxConfig trespasser)
        {
            BunkerSceneNames.Clear();
            Il2CppTLD.Gameplay.BunkerInteriorSpecification[] interiors = trespasser.m_BunkerSetup.m_BunkerInteriors;
            if (interiors == null) return;

            foreach (Il2CppTLD.Gameplay.BunkerInteriorSpecification spec in interiors)
            {
                if (spec?.m_Interior == null) continue;
                string sceneName = spec.m_Interior.name;
                if (string.IsNullOrEmpty(sceneName)) continue;
                BunkerSceneNames.Add(sceneName);
            }
        }

        #endregion

        #region XP Mode

        private void ConfigureSandboxXPMode(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode = UnityEngine.Object.Instantiate(interloper.m_XPMode);
            trespasser.m_XPMode.m_ModeType = ExperienceModeType.Interloper;

            ConfigureDifficultySettings(trespasser);
            ConfigureSandboxStartGear(trespasser, stalker, interloper);
            ConfigureSandboxCougarSettings(trespasser, stalker, interloper);
        }

        private void ConfigureDifficultySettings(SandboxConfig trespasser)
        {
            var xp = trespasser.m_XPMode;

            xp.m_WeatherDurationScale = 0.325f;
            xp.m_ChanceOfBlizzardScale = 1.775f;
            xp.m_FreezingRateScale = 1.585f;
            xp.m_FrostbiteDamageMultiplier = 1.425f;
            xp.m_OutdoorTempDropCelsiusMax = 17f;
            xp.m_OutdoorTempDropDayStart = 10;
            xp.m_OutdoorTempDropDayFinal = 95;
            xp.m_NumHoursWarmForHypothermiaCureScale = 1.85f;
            xp.m_ClosestSpawnDistanceAfterTransitionScale = 0.6f;  
            xp.m_RespawnHoursScaleMax = 3.2f;
            xp.m_RespawnHoursScaleDayFinal = 90;
            xp.m_RadialRespawnTimeScaleMax = 2.6f;
            xp.m_RadialRespawnTimeScaleDayStart = 7;
            xp.m_RadialRespawnTimeScaleDayFinal = 110;
            xp.m_FishCatchTimeScaleMax = 1.6f;
            xp.m_FishCatchTimeScaleDayStart = 6;
            xp.m_FishCatchTimeScaleDayFinal = 31;
            xp.m_CalorieBurnScale = 0.95f;
            xp.m_ThirstRateScale = 1.15f;
            xp.m_FatigueRateScale = 0.85f;
            xp.m_ConditonRecoveryFromRestScale = 0.65f;
            xp.m_ConditonRecoveryWhileAwakeScale = 0.65f;
            xp.m_DecayScale = 1.65f;
            xp.m_GearSpawnChanceScale = 0.275f;
            xp.m_ChanceForEmptyContainer = 82;
            xp.m_StruggleTapStrengthScale = 0.88f;
            xp.m_StrugglePlayerDamageReceivedScale = 1.325f;
            xp.m_StrugglePlayerClothingDamageScale = 1.325f;
            xp.m_IntestinalParasitesNumberOfRemedyDoses = 17;
            xp.m_DaysBeforeCabinFeverRiskOnset = 11;
        }

        private void ConfigureSandboxCougarSettings(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode.m_CougarSettings = UnityEngine.Object.Instantiate(interloper.m_XPMode.m_CougarSettings);
            trespasser.m_XPMode.m_CougarSettings.m_ArrivalTimeMinimumDays = 14f;
            trespasser.m_XPMode.m_CougarSettings.m_ArrivalTimeGuaranteedDays = 16f;
            trespasser.m_XPMode.m_CougarSettings.m_RespawnCooldownDays = 12f;
        }

        #endregion

        #region Start Gear

        private static readonly (string prefab, int count)[] mStartGear =
        {
            ("GEAR_PackMatches",         3),
            ("GEAR_WaterSupplyPotable",  1),
            ("GEAR_CandyBar",            1),
            ("GEAR_BasicWoolHat",        1),
            ("GEAR_LongUnderwear",       1),
            ("GEAR_WoolSocks",           1),
            ("GEAR_BasicShoes",          1),
            ("GEAR_WorkPants",           1),
            ("GEAR_SkiJacket",           1),
            ("GEAR_RecycledCan",         1),
        };

        private void ConfigureSandboxStartGear(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode.m_StartGear = UnityEngine.Object.Instantiate(interloper.m_XPMode.m_StartGear);
            trespasser.m_XPMode.m_StartGear.m_FixedGear = BuildStartGear();
            trespasser.m_XPMode.m_StartGear.m_RandomGear = new Il2CppReferenceArray<RandomGearItem>(0);
        }

        private Il2CppReferenceArray<FixedGearItem> BuildStartGear()
        {
            List<FixedGearItem> items = new();
            foreach ((string prefabName, int count) in mStartGear)
            {
                GameObject prefab = AssetHelper.SafeLoadAssetAsync<GameObject>(prefabName).WaitForCompletion();
                GearItem? gearItem = prefab == null ? null : prefab.GetComponent<GearItem>();
                if (gearItem == null || gearItem.m_GearItemData == null) continue;
                items.Add(new FixedGearItem() { m_GearItem = gearItem.m_GearItemData.PrefabReference, m_Count = count });
            }
            return new Il2CppReferenceArray<FixedGearItem>(items.ToArray());
        }

        #endregion

        #region Regions & Load Conditions

        private void ConfigureSandboxAvailableRegions(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_AvailableStartRegions = new Il2CppReferenceArray<Il2CppTLD.Scenes.RegionSpecification>(interloper.m_AvailableStartRegions.Length);
            for (int i = 0, iMax = interloper.m_AvailableStartRegions.Length; i < iMax; i++)
            {
                trespasser.m_AvailableStartRegions[i] = interloper.m_AvailableStartRegions[i];
            }
        }

        private void ConfigureSandboxSceneLoadConditions(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_LoadConditions = new Il2CppReferenceArray<Il2CppTLD.Scenes.SceneLoadCondition>(interloper.m_LoadConditions.Length);
            for (int i = 0, iMax = interloper.m_LoadConditions.Length; i < iMax; i++)
            {
                trespasser.m_LoadConditions[i] = interloper.m_LoadConditions[i];
            }
        }

        #endregion
    }
}
