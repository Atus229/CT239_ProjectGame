using System;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        public const string TypeDayTimeBudget = "DayTimeBudget";
        public const string TypeVehicleCapacity = "VehicleCapacity";
        public const string TypeHandlingEfficiency = "HandlingEfficiency";
        public const string TypeLateGraceBuffer = "LateGraceBuffer";

        [SerializeField] private int upgradeID;
        [SerializeField] private string upgradeName;
        [SerializeField] private string upgradeType;

        [SerializeField] [Min(1)] private int baseCost = 100;
        [SerializeField] [Min(1f)] private float costGrowth = 2.0f;
        [SerializeField] [Min(1)] private int maxLevel = 3;

        [SerializeField] private float baseEffectValue = 30f;
        [SerializeField] [TextArea(2, 4)] private string description;
        [SerializeField] private Sprite icon;

        public int UpgradeID => upgradeID;
        public string UpgradeName => upgradeName;
        public string UpgradeType => upgradeType;
        public int BaseCost => baseCost;
        public float CostGrowth => costGrowth;
        public int MaxLevel => maxLevel;
        public float BaseEffectValue => baseEffectValue;
        public string Description => description;
        public Sprite Icon => icon;

        public int GetCostForLevel(int targetLevel)
        {
            if (targetLevel <= 0 || targetLevel > maxLevel)
            {
                return -1;
            }

            if (targetLevel == 1)
            {
                return baseCost;
            }

            float calculatedCost = baseCost * Mathf.Pow(costGrowth, targetLevel - 1);
            return Mathf.RoundToInt(calculatedCost);
        }

        public float GetEffectValue(int currentLevel)
        {
            if (currentLevel <= 0) return 0f;
            int clampedLevel = Mathf.Min(currentLevel, maxLevel);
            return baseEffectValue * clampedLevel;
        }

        public bool IsMaxLevel(int currentLevel)
        {
            return currentLevel >= maxLevel;
        }

        public void Init(int id, string name, string type, int baseCostVal, float growth, int maxLvl, float effectVal, string desc = "")
        {
            this.upgradeID = id;
            this.upgradeName = name;
            this.upgradeType = type;
            this.baseCost = baseCostVal;
            this.costGrowth = growth;
            this.maxLevel = maxLvl;
            this.baseEffectValue = effectVal;
            this.description = desc;
        }
    }
}
