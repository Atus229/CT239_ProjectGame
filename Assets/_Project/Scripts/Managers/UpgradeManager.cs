using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.Data;

namespace _Project.Scripts.Managers
{
    /// <summary>
    /// Quản lý toàn bộ hệ thống nâng cấp trong game.
    /// Chịu trách nhiệm kiểm tra điều kiện mua, trừ tiền, nâng cấp và cung cấp chỉ số hiệu ứng cho gameplay.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }
        
        [SerializeField] private List<UpgradeData> availableUpgrades = new List<UpgradeData>();
        [SerializeField] private List<Player_Upgrade> playerUpgrades = new List<Player_Upgrade>();
        [SerializeField] private int currentPlayerID = 1;

        private readonly Dictionary<int, UpgradeData> upgradeDataMap = new Dictionary<int, UpgradeData>();
        private readonly Dictionary<int, Player_Upgrade> playerUpgradeMap = new Dictionary<int, Player_Upgrade>();

        public IReadOnlyList<UpgradeData> AvailableUpgrades => availableUpgrades;
        public IReadOnlyList<Player_Upgrade> PlayerUpgrades => playerUpgrades;
        public int CurrentPlayerID
        {
            get => currentPlayerID;
            set => currentPlayerID = value;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCatalogAndState();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void InitializeCatalogAndState()
        {
            upgradeDataMap.Clear();
            playerUpgradeMap.Clear();

            foreach (var upg in availableUpgrades)
            {
                if (upg != null && !upgradeDataMap.ContainsKey(upg.UpgradeID))
                {
                    upgradeDataMap.Add(upg.UpgradeID, upg);
                }
            }

            foreach (var pUpg in playerUpgrades)
            {
                if (pUpg != null && !playerUpgradeMap.ContainsKey(pUpg.UpgradeID))
                {
                    playerUpgradeMap.Add(pUpg.UpgradeID, pUpg);
                }
            }

            int nextPlayerUpgradeId = playerUpgrades.Count + 1;
            foreach (var upg in availableUpgrades)
            {
                if (upg != null && !playerUpgradeMap.ContainsKey(upg.UpgradeID))
                {
                    var newRecord = new Player_Upgrade(nextPlayerUpgradeId++, currentPlayerID, upg.UpgradeID, 0);
                    playerUpgrades.Add(newRecord);
                    playerUpgradeMap.Add(upg.UpgradeID, newRecord);
                }
            }
        }

        public void SetAvailableUpgrades(List<UpgradeData> upgrades)
        {
            this.availableUpgrades = upgrades ?? new List<UpgradeData>();
            InitializeCatalogAndState();
        }

        public Player_Upgrade GetPlayerUpgrade(int upgradeId)
        {
            playerUpgradeMap.TryGetValue(upgradeId, out var record);
            return record;
        }

        public int GetUpgradeLevel(int upgradeId)
        {
            var record = GetPlayerUpgrade(upgradeId);
            return record != null ? record.CurrentLevel : 0;
        }

        public int GetUpgradeLevelByType(string upgradeType)
        {
            var data = GetUpgradeDataByType(upgradeType);
            return data != null ? GetUpgradeLevel(data.UpgradeID) : 0;
        }

        public UpgradeData GetUpgradeData(int upgradeId)
        {
            upgradeDataMap.TryGetValue(upgradeId, out var data);
            return data;
        }

        public UpgradeData GetUpgradeDataByType(string upgradeType)
        {
            foreach (var data in availableUpgrades)
            {
                if (data != null && string.Equals(data.UpgradeType, upgradeType, StringComparison.OrdinalIgnoreCase))
                {
                    return data;
                }
            }
            return null;
        }

        public int GetNextUpgradeCost(int upgradeId)
        {
            var data = GetUpgradeData(upgradeId);
            if (data == null) return -1;

            int currentLevel = GetUpgradeLevel(upgradeId);
            if (data.IsMaxLevel(currentLevel)) return -1;

            return data.GetCostForLevel(currentLevel + 1);
        }

        public bool CanUpgrade(int upgradeId, int currentMoney)
        {
            var data = GetUpgradeData(upgradeId);
            if (data == null) return false;

            int currentLevel = GetUpgradeLevel(upgradeId);
            if (data.IsMaxLevel(currentLevel)) return false;

            int nextCost = data.GetCostForLevel(currentLevel + 1);
            if (nextCost < 0) return false;

            return currentMoney >= nextCost;
        }

        public bool TryPurchaseUpgrade(int upgradeId, ref int currentMoney)
        {
            if (!CanUpgrade(upgradeId, currentMoney))
            {
                return false;
            }

            var data = GetUpgradeData(upgradeId);
            var record = GetPlayerUpgrade(upgradeId);
            if (data == null || record == null)
            {
                return false;
            }

            int targetLevel = record.CurrentLevel + 1;
            int cost = data.GetCostForLevel(targetLevel);

            currentMoney -= cost;
            record.CurrentLevel = targetLevel;

            EventBus.Publish(new GameEvent.UpgradePurchasedEvent(
                record.PlayerUpgradeID,
                data.UpgradeID,
                record.CurrentLevel,
                cost,
                data.UpgradeType
            ));

            return true;
        }

        #region Gameplay Effect Getters

        public float GetBonusDayTimeBudget()
        {
            var data = GetUpgradeDataByType(UpgradeData.TypeDayTimeBudget);
            if (data == null) return 0f;
            int level = GetUpgradeLevel(data.UpgradeID);
            return data.GetEffectValue(level);
        }

        public int GetBonusGridColumns()
        {
            var data = GetUpgradeDataByType(UpgradeData.TypeVehicleCapacity);
            if (data == null) return 0;
            int level = GetUpgradeLevel(data.UpgradeID);
            return Mathf.RoundToInt(data.GetEffectValue(level));
        }

        public float GetHandlingTimeMultiplier()
        {
            var data = GetUpgradeDataByType(UpgradeData.TypeHandlingEfficiency);
            if (data == null) return 1.0f;
            int level = GetUpgradeLevel(data.UpgradeID);
            float reductionPercent = data.GetEffectValue(level);
            return Mathf.Max(0.1f, 1.0f - reductionPercent);
        }

        public float GetLateGraceBuffer()
        {
            var data = GetUpgradeDataByType(UpgradeData.TypeLateGraceBuffer);
            if (data == null) return 0f;
            int level = GetUpgradeLevel(data.UpgradeID);
            return data.GetEffectValue(level);
        }

        #endregion

        #region Save / Load Integration
        public List<Player_Upgrade> ExportPlayerUpgrades()
        {
            var exportList = new List<Player_Upgrade>();
            foreach (var record in playerUpgrades)
            {
                if (record != null)
                {
                    exportList.Add(new Player_Upgrade(record.PlayerUpgradeID, record.PlayerID, record.UpgradeID, record.CurrentLevel));
                }
            }
            return exportList;
        }

        public void ImportPlayerUpgrades(List<Player_Upgrade> loadedUpgrades)
        {
            if (loadedUpgrades == null) return;

            playerUpgrades.Clear();
            playerUpgradeMap.Clear();

            foreach (var item in loadedUpgrades)
            {
                if (item != null)
                {
                    playerUpgrades.Add(item);
                    playerUpgradeMap[item.UpgradeID] = item;
                }
            }

            InitializeCatalogAndState();
        }
        
        public void ResetAllUpgrades()
        {
            foreach (var record in playerUpgrades)
            {
                if (record != null)
                {
                    record.CurrentLevel = 0;
                }
            }
        }
        
        #endregion
    }
}
