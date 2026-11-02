using System;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [Serializable]
    public class Player_Upgrade
    {
        [SerializeField] private int playerUpgradeID;
        [SerializeField] private int playerID;
        [SerializeField] private int upgradeID;
        [SerializeField] private int currentLevel;

        public int PlayerUpgradeID
        {
            get => playerUpgradeID;
            set => playerUpgradeID = value;
        }

        public int PlayerID
        {
            get => playerID;
            set => playerID = value;
        }

        public int UpgradeID
        {
            get => upgradeID;
            set => upgradeID = value;
        }

        public int CurrentLevel
        {
            get => currentLevel;
            set => currentLevel = value;
        }

        public Player_Upgrade()
        {
        }

        public Player_Upgrade(int playerUpgradeID, int playerID, int upgradeID, int currentLevel = 0)
        {
            this.playerUpgradeID = playerUpgradeID;
            this.playerID = playerID;
            this.upgradeID = upgradeID;
            this.currentLevel = currentLevel;
        }
    }
}
