using System.Reflection;
using Il2Cpp;
using Il2CppTLD.Gameplay;

namespace Trespasser
{
    internal class CustomLevels
    {
        public const string LevelLabel = "Trespasser";

        private class LevelSetting
        {
            public string TunableProperty;
            public string[] ExperienceModeFields;
            public string ComboProperty;
            public int StalkerLevel;

            public LevelSetting(string tunableProperty, string[] experienceModeFields, string comboProperty, int stalkerLevel)
            {
                TunableProperty = tunableProperty;
                ExperienceModeFields = experienceModeFields;
                ComboProperty = comboProperty;
                StalkerLevel = stalkerLevel;
            }
        }

        private static readonly LevelSetting[] mSettings =
        {
            new("m_StruggleClothingDamageBonus", new[] { "m_StrugglePlayerClothingDamageScale" }, "m_StruggleClothingDamagePopupList", 1),
            new("m_WeatherChangeFrequency", new[] { "m_WeatherDurationScale" }, "m_WeatherChangeFreqPopupList", 2),
            new("m_StrugglePlayerStrengthBonus", new[] { "m_StruggleTapStrengthScale" }, "m_StrugglePlayerStrengthBonusPopupList", 1),
            new("m_GradualTempReductionRate", new[] { "m_OutdoorTempDropCelsiusMax", "m_OutdoorTempDropDayStart", "m_OutdoorTempDropDayFinal" }, "m_GradualTempReductionPopupList", 2),
            new("m_CalorieBurnRate", new[] { "m_CalorieBurnScale" }, "m_CalorieBurnRatePopupList", 2),
            new("m_ThirstIncrease", new[] { "m_ThirstRateScale" }, "m_ThirstIncreasePopupList", 1),
            new("m_FatigueIncrease", new[] { "m_FatigueRateScale" }, "m_FatigueIncreasePopupList", 2),
            new("m_FreezingIncrease", new[] { "m_FreezingRateScale" }, "m_FreezingIncreasePopupList", 2),
            new("m_HoursWarmthToCureHypothermia", new[] { "m_NumHoursWarmForHypothermiaCureScale" }, "m_HoursToCureHypothermiaPopupList", 2),
            new("m_ItemDecayRate", new[] { "m_DecayScale" }, "m_ItemDecayRatePopupList", 2),
            new("m_ItemSpawnChance", new[] { "m_GearSpawnChanceScale" }, "m_ItemSpawnChancePopupList", 1),
            new("m_ReduceWildlifePopOverTime", new[] { "m_RespawnHoursScaleMax", "m_RespawnHoursScaleDayStart", "m_RespawnHoursScaleDayFinal" }, "m_ReduceWildlifePopOverTimePopupList", 2),
            new("m_EmptyContainerChance", new[] { "m_ChanceForEmptyContainer" }, "m_EmptyContainerChancePopupList", 2),
            new("m_StickBranchStoneSpawnFrequency", new[] { "m_RadialRespawnTimeScaleMax", "m_RadialRespawnTimeScaleDayStart", "m_RadialRespawnTimeScaleDayFinal" }, "m_StickBranchStoneFreqPopupList", 1),
            new("m_StruggleDamageReceivedBonus", new[] { "m_StrugglePlayerDamageReceivedScale" }, "m_StruggleDamageReceivedModifierPopupList", 0),
            new("m_ConditionRecoveryRest", new[] { "m_ConditonRecoveryFromRestScale" }, "m_ConditionRecoveryRestPopupList", 3),
            new("m_ConditionRecoveryAwake", new[] { "m_ConditonRecoveryWhileAwakeScale" }, "m_ConditionRecoveryAwakePopupList", 3),
            new("m_FrosbiteRisk", new[] { "m_FrostbiteDamageMultiplier" }, "m_FrostbiteRiskPopupList", 3),
            new("m_BlizzardFrequency", new[] { "m_ChanceOfBlizzardScale" }, "m_BlizzardFreqPopupList", 3),
            new("m_WolfSpawnDistance", new[] { "m_ClosestSpawnDistanceAfterTransitionScale" }, "m_WolfSpawnDistancePopupList", 0),
        };

        private TrespasserManager mManager;
        private Dictionary<IntPtr, List<LevelSetting>> mSuspendedLevels = new();

        public CustomLevels(TrespasserManager manager)
        {
            mManager = manager;
        }

        public int Count => mSettings.Length;


        public void AddOptions(Panel_CustomXPSetup panel)
        {
            foreach (LevelSetting setting in mSettings)
            {
                ConsoleComboBox combo = typeof(Panel_CustomXPSetup).GetProperty(setting.ComboProperty)?.GetValue(panel) as ConsoleComboBox;
                if (combo?.items == null || combo.items.Contains(LevelLabel)) continue;
                if (combo.items.Count != TrespasserLevelOf(setting)) continue;
                combo.items.Add(LevelLabel);
            }
        }


        public void Suspend(CustomExperienceMode customMode)
        {
            if (customMode == null || mSuspendedLevels.ContainsKey(customMode.Pointer)) return;

            List<LevelSetting> suspended = mSettings.Where(setting => IsTrespasserLevel(customMode, setting)).ToList();
            foreach (LevelSetting setting in suspended)
                SetLevel(customMode, setting, setting.StalkerLevel);
            mSuspendedLevels[customMode.Pointer] = suspended;
        }


        public void Restore(CustomExperienceMode customMode)
        {
            if (customMode == null || !mSuspendedLevels.Remove(customMode.Pointer, out List<LevelSetting> suspended)) return;

            foreach (LevelSetting setting in suspended)
                SetLevel(customMode, setting, TrespasserLevelOf(setting));
        }


        public void ApplyTrespasserValues(CustomExperienceMode customMode)
        {
            ExperienceMode trespasserMode = mManager.SandboxConfigManager.TrespasserConfig.m_XPMode;
            if (customMode == null || trespasserMode == null) return;

            foreach (LevelSetting setting in mSettings.Where(setting => IsTrespasserLevel(customMode, setting)))
            {
                foreach (string fieldName in setting.ExperienceModeFields)
                {
                    PropertyInfo property = typeof(ExperienceMode).GetProperty(fieldName);
                    property.SetValue(customMode, property.GetValue(trespasserMode));
                }
            }
        }


        public List<string> TrespasserTunables(CustomExperienceMode customMode)
            => mSettings.Where(setting => IsTrespasserLevel(customMode, setting)).Select(setting => setting.TunableProperty).ToList();


        public void SetToTrespasser(CustomExperienceMode customMode, ICollection<string> tunableProperties)
        {
            foreach (LevelSetting setting in mSettings.Where(setting => tunableProperties.Contains(setting.TunableProperty)))
                SetLevel(customMode, setting, TrespasserLevelOf(setting));
        }


        private bool IsTrespasserLevel(CustomExperienceMode customMode, LevelSetting setting)
            => Convert.ToInt32(TunableProperty(setting).GetValue(customMode)) == TrespasserLevelOf(setting);


        private int TrespasserLevelOf(LevelSetting setting) => Enum.GetValues(TunableProperty(setting).PropertyType).Length;


        private void SetLevel(CustomExperienceMode customMode, LevelSetting setting, int level)
        {
            PropertyInfo property = TunableProperty(setting);
            property.SetValue(customMode, Enum.ToObject(property.PropertyType, level));
        }


        private PropertyInfo TunableProperty(LevelSetting setting) => typeof(CustomExperienceMode).GetProperty(setting.TunableProperty);
    }
}
