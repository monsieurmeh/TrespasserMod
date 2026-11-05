using System.Reflection;
using Il2Cpp;
using Il2CppTLD.AI;
using Il2CppTLD.Gameplay;

namespace Trespasser
{
    internal class CustomLevels
    {
        public const string LevelLabel = "Trespasser";
        public const string VeryLow = "Very Low";
        public const string MediumLow = "Medium-Low";
        public const string MediumHigh = "Medium-High";
        public const string Higher = "Higher";
        public const string Nearby = "Nearby";
        private const string CougarTunableProperty = "m_CougarDifficulty";

        private class LevelSetting
        {
            public string TunableProperty;
            public string[] ExperienceModeFields;
            public string ComboProperty;
            public int StalkerLevel;
            public int ShownIndex;
            public string Name;

            public LevelSetting(string tunableProperty, string[] experienceModeFields, string comboProperty, int stalkerLevel, int shownIndex, string name)
            {
                TunableProperty = tunableProperty;
                ExperienceModeFields = experienceModeFields;
                ComboProperty = comboProperty;
                StalkerLevel = stalkerLevel;
                ShownIndex = shownIndex;
                Name = name;
            }
        }

        private static readonly LevelSetting[] mSettings =
        {
            new("m_StruggleClothingDamageBonus", new[] { "m_StrugglePlayerClothingDamageScale" }, "m_StruggleClothingDamagePopupList", 1, 3, MediumHigh),
            new("m_WeatherChangeFrequency", new[] { "m_WeatherDurationScale" }, "m_WeatherChangeFreqPopupList", 2, 3, Higher),
            new("m_StrugglePlayerStrengthBonus", new[] { "m_StruggleTapStrengthScale" }, "m_StrugglePlayerStrengthBonusPopupList", 1, 1, VeryLow),
            new("m_GradualTempReductionRate", new[] { "m_OutdoorTempDropCelsiusMax", "m_OutdoorTempDropDayStart", "m_OutdoorTempDropDayFinal" }, "m_GradualTempReductionPopupList", 2, 3, MediumHigh),
            new("m_CalorieBurnRate", new[] { "m_CalorieBurnScale" }, "m_CalorieBurnRatePopupList", 2, 3, Higher),
            new("m_ThirstIncrease", new[] { "m_ThirstRateScale" }, "m_ThirstIncreasePopupList", 1, 2, MediumHigh),
            new("m_FatigueIncrease", new[] { "m_FatigueRateScale" }, "m_FatigueIncreasePopupList", 2, 2, MediumHigh),
            new("m_FreezingIncrease", new[] { "m_FreezingRateScale" }, "m_FreezingIncreasePopupList", 2, 3, Higher),
            new("m_HoursWarmthToCureHypothermia", new[] { "m_NumHoursWarmForHypothermiaCureScale" }, "m_HoursToCureHypothermiaPopupList", 2, 3, Higher),
            new("m_ItemDecayRate", new[] { "m_DecayScale" }, "m_ItemDecayRatePopupList", 2, 3, Higher),
            new("m_ItemSpawnChance", new[] { "m_GearSpawnChanceScale" }, "m_ItemSpawnChancePopupList", 1, 1, MediumLow),
            new("m_ReduceWildlifePopOverTime", new[] { "m_RespawnHoursScaleMax", "m_RespawnHoursScaleDayStart", "m_RespawnHoursScaleDayFinal" }, "m_ReduceWildlifePopOverTimePopupList", 2, 3, MediumHigh),
            new("m_EmptyContainerChance", new[] { "m_ChanceForEmptyContainer" }, "m_EmptyContainerChancePopupList", 2, 3, MediumHigh),
            new("m_StickBranchStoneSpawnFrequency", new[] { "m_RadialRespawnTimeScaleMax", "m_RadialRespawnTimeScaleDayStart", "m_RadialRespawnTimeScaleDayFinal" }, "m_StickBranchStoneFreqPopupList", 1, 1, MediumLow),
            new("m_StruggleDamageReceivedBonus", new[] { "m_StrugglePlayerDamageReceivedScale" }, "m_StruggleDamageReceivedModifierPopupList", 0, 3, MediumHigh),
            new("m_ConditionRecoveryRest", new[] { "m_ConditonRecoveryFromRestScale" }, "m_ConditionRecoveryRestPopupList", 3, 3, MediumHigh),
            new("m_ConditionRecoveryAwake", new[] { "m_ConditonRecoveryWhileAwakeScale" }, "m_ConditionRecoveryAwakePopupList", 3, 3, MediumHigh),
            new("m_FrosbiteRisk", new[] { "m_FrostbiteDamageMultiplier" }, "m_FrostbiteRiskPopupList", 3, 4, Higher),
            new("m_BlizzardFrequency", new[] { "m_ChanceOfBlizzardScale" }, "m_BlizzardFreqPopupList", 3, 4, Higher),
            new("m_WolfSpawnDistance", new[] { "m_ClosestSpawnDistanceAfterTransitionScale" }, "m_WolfSpawnDistancePopupList", 0, 1, Nearby),
            new("m_StartingGear", new[] { "m_StartGear" }, "m_StartingGearPopupList", 1, 1, MediumLow),
            new(CougarTunableProperty, Array.Empty<string>(), "m_CougarDifficultyPopupList", 2, 3, MediumHigh),
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
                ConsoleComboBox? combo = ComboOf(panel, setting);
                if (combo?.items == null || combo.items.Contains(setting.Name)) continue;
                if (combo.items.Count != TrespasserLevelOf(setting)) continue;
                combo.items.Insert(setting.ShownIndex, setting.Name);
            }
        }


        public void BeginReadFromUI(Panel_CustomXPSetup panel)
        {
            foreach (LevelSetting setting in mSettings)
            {
                ConsoleComboBox? combo = ComboOf(panel, setting);
                if (combo?.items == null || !combo.items.Contains(setting.Name)) continue;
                combo.m_CurrentIndex = LevelOfShown(setting, combo.m_CurrentIndex);
            }
        }


        public void EndReadFromUI(Panel_CustomXPSetup panel)
        {
            foreach (LevelSetting setting in mSettings)
            {
                ConsoleComboBox? combo = ComboOf(panel, setting);
                if (combo?.items == null || !combo.items.Contains(setting.Name)) continue;
                combo.m_CurrentIndex = combo.items.IndexOf(combo.value);
            }
        }


        public void ShowInUI(Panel_CustomXPSetup panel)
        {
            CustomExperienceMode customMode = panel.m_CustomMode;
            if (customMode == null) return;
            foreach (LevelSetting setting in mSettings)
            {
                ConsoleComboBox? combo = ComboOf(panel, setting);
                if (combo?.items == null || !combo.items.Contains(setting.Name)) continue;
                int level = Convert.ToInt32(TunableProperty(setting).GetValue(customMode));
                combo.value = combo.items[ShownOfLevel(setting, level)];
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


        public CougarSettings? TrespasserCougarSettings(CustomExperienceMode customMode)
        {
            LevelSetting cougar = mSettings.First(setting => setting.TunableProperty == CougarTunableProperty);
            if (customMode == null || !IsTrespasserLevel(customMode, cougar)) return null;
            return mManager.SandboxConfigManager.TrespasserConfig.m_XPMode.GetCougarSettings(false);
        }


        public List<string> TrespasserTunables(CustomExperienceMode customMode)
            => mSettings.Where(setting => IsTrespasserLevel(customMode, setting)).Select(setting => setting.TunableProperty).ToList();


        public void SetAllToTrespasser(CustomExperienceMode customMode)
        {
            foreach (LevelSetting setting in mSettings)
                SetLevel(customMode, setting, TrespasserLevelOf(setting));
        }


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


        private int LevelOfShown(LevelSetting setting, int shown)
            => shown == setting.ShownIndex ? TrespasserLevelOf(setting) : shown < setting.ShownIndex ? shown : shown - 1;


        private int ShownOfLevel(LevelSetting setting, int level)
            => level == TrespasserLevelOf(setting) ? setting.ShownIndex : level < setting.ShownIndex ? level : level + 1;


        private static ConsoleComboBox? ComboOf(Panel_CustomXPSetup panel, LevelSetting setting)
            => typeof(Panel_CustomXPSetup).GetProperty(setting.ComboProperty)?.GetValue(panel) as ConsoleComboBox;
    }
}
