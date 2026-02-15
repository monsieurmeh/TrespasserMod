using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTLD.AI;
using Il2CppTLD.Gameplay;
using Il2CppTLD.Gear;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trespasser
{
    internal class SandboxConfigManager
    {
        private static SandboxConfig mTrespasserConfig;

        internal static HashSet<string> BunkerSceneNames { get; } = new(StringComparer.OrdinalIgnoreCase);

        public static SandboxConfig TrespasserConfig
        {
            get
            {
                if (mTrespasserConfig == null)
                    mTrespasserConfig = InstantiateSandbox();
                return mTrespasserConfig;
            }
            set => mTrespasserConfig = value;
        }

        #region Top-Level Configuration

        private static SandboxConfig InstantiateSandbox()
        {
            var trespasser = new SandboxConfig();
            var stalker = ExperienceModeManager.GetGameModeFromName("Stalker").Cast<SandboxConfig>();
            var interloper = ExperienceModeManager.GetGameModeFromName("Interloper").Cast<SandboxConfig>();

            ConfigureSandbox(trespasser, stalker, interloper);
            return trespasser;
        }

        private static void ConfigureSandbox(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            ConfigureSandboxGeneral(trespasser, stalker, interloper);
            ConfigureSandboxBunkerSetup(trespasser, stalker, interloper);
            ConfigureSandboxXPMode(trespasser, stalker, interloper);
            ConfigureSandboxAvailableRegions(trespasser, stalker, interloper);
            ConfigureSandboxSceneLoadConditions(trespasser, stalker, interloper);
        }

        #endregion

        #region General Settings

        private static void ConfigureSandboxGeneral(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
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

        private static void ConfigureSandboxBunkerSetup(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_BunkerSetup = UnityEngine.Object.Instantiate(stalker.m_BunkerSetup);
            CollectBunkerSceneNames(trespasser);
        }

        private static void CollectBunkerSceneNames(SandboxConfig trespasser)
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
                MelonLogger.Msg($"[Trespasser] Registered bunker scene: {sceneName}");
            }
        }

        #endregion

        #region XP Mode

        private static void ConfigureSandboxXPMode(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode = UnityEngine.Object.Instantiate(interloper.m_XPMode);
            trespasser.m_XPMode.m_ModeType = ExperienceModeType.Interloper;

            ConfigureDifficultySettings(trespasser);
            ConfigureSandboxStartGear(trespasser, stalker, interloper);
            ConfigureSandboxCougarSettings(trespasser, stalker, interloper);
        }

        private static void ConfigureDifficultySettings(SandboxConfig trespasser)
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

        private static void ConfigureSandboxCougarSettings(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode.m_CougarSettings = UnityEngine.Object.Instantiate(interloper.m_XPMode.m_CougarSettings);
            trespasser.m_XPMode.m_CougarSettings.m_ArrivalTimeMinimumDays = 14f;
            trespasser.m_XPMode.m_CougarSettings.m_ArrivalTimeGuaranteedDays = 16f;
            trespasser.m_XPMode.m_CougarSettings.m_RespawnCooldownDays = 12f;
        }

        #endregion

        #region Start Gear

        private static void ConfigureSandboxStartGear(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_XPMode.m_StartGear = UnityEngine.Object.Instantiate(interloper.m_XPMode.m_StartGear);

            var fixedGear = BuildTrespasserFixedGear(stalker);
            if (fixedGear != null)
                trespasser.m_XPMode.m_StartGear.m_FixedGear = fixedGear;
        }

        private static readonly (string prefab, int count)[] mGearDefinitions =
        {
            ("GEAR_PackMatches",         3),
            ("GEAR_WaterSupplyPotable",  1),
            ("GEAR_CandyBar",            1),
            ("GEAR_BasicWoolHat",        1),
            ("GEAR_LongUnderwear",       1),
            ("GEAR_WoolSocks",           1),
            ("GEAR_BasicShoes",          1),
            ("GEAR_WorkPants",           1),
            ("GEAR_LightShell",          1),
            ("GEAR_RecycledCan",         1),
        };

        private static Il2CppReferenceArray<FixedGearItem>? BuildTrespasserFixedGear(SandboxConfig stalker)
        {
            try
            {
                var stalkerGearLookup = BuildStalkerGearLookup(stalker);
                if (stalkerGearLookup.Count == 0)
                {
                    return null;
                }

                var items = new List<FixedGearItem>();
                foreach (var (prefab, count) in mGearDefinitions)
                {
                    var fixedItem = ResolveFixedGearItem(prefab, count, stalkerGearLookup);
                    if (fixedItem != null)
                    {
                        items.Add(fixedItem);
                    }
                }

                if (items.Count == 0)
                {
                    return null;
                }

                var result = new Il2CppReferenceArray<FixedGearItem>(items.Count);
                for (int i = 0; i < items.Count; i++)
                    result[i] = items[i];

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static Dictionary<string, AssetReferenceGearItem> BuildStalkerGearLookup(SandboxConfig stalker)
        {
            var lookup = new Dictionary<string, AssetReferenceGearItem>(StringComparer.OrdinalIgnoreCase);

            var stalkerStartGear = stalker.m_XPMode?.m_StartGear;
            if (stalkerStartGear == null) return lookup;

            var fixedGear = stalkerStartGear.m_FixedGear;
            if (fixedGear == null) return lookup;

            for (int i = 0; i < fixedGear.Length; i++)
            {
                var entry = fixedGear[i];
                if (entry?.m_GearItem == null) continue;

                var prefabName = TryResolvePrefabName(entry.m_GearItem);
                if (string.IsNullOrEmpty(prefabName)) continue;

                lookup.TryAdd(prefabName, entry.m_GearItem);
            }

            return lookup;
        }

        private static string? TryResolvePrefabName(AssetReferenceGearItem assetRef)
        {
            try
            {
                if (!assetRef.RuntimeKeyIsValid()) return null;

                var gameObj = assetRef.GetOrLoadAsset();
                if (gameObj == null) return null;

                return gameObj.name;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private static FixedGearItem? ResolveFixedGearItem(string prefabName, int count, Dictionary<string, AssetReferenceGearItem> stalkerLookup)
        {
            if (stalkerLookup.TryGetValue(prefabName, out var stalkerRef))
                return CreateFixedGearItem(stalkerRef, count);

            return TryCreateFixedGearItemFromPrefab(prefabName, count);
        }


        private static FixedGearItem? TryCreateFixedGearItemFromPrefab(string prefabName, int count)
        {
            try
            {
                var gearPrefab = GearItem.LoadGearItemPrefab(prefabName);
                if (gearPrefab == null)
                {
                    return null;
                }

                var gearItemData = gearPrefab.m_GearItemData;
                if (gearItemData == null)
                {
                    return null;
                }

                var prefabRef = gearItemData.PrefabReference;
                if (prefabRef == null)
                {
                    return null;
                }

                return CreateFixedGearItem(prefabRef, count);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static FixedGearItem CreateFixedGearItem(AssetReferenceGearItem assetRef, int count)
        {
            var item = new FixedGearItem();
            item.m_GearItem = assetRef;
            item.m_Count = count;
            return item;
        }

        #endregion

        #region Regions & Load Conditions

        private static void ConfigureSandboxAvailableRegions(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
        {
            trespasser.m_AvailableStartRegions = new Il2CppReferenceArray<Il2CppTLD.Scenes.RegionSpecification>(interloper.m_AvailableStartRegions.Length);
            for (int i = 0, iMax = interloper.m_AvailableStartRegions.Length; i < iMax; i++)
            {
                trespasser.m_AvailableStartRegions[i] = interloper.m_AvailableStartRegions[i];
            }
        }

        private static void ConfigureSandboxSceneLoadConditions(SandboxConfig trespasser, SandboxConfig stalker, SandboxConfig interloper)
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
