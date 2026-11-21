using System;
using System.Collections.Generic;

namespace _Project.Scripts.Data
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int currentDayIndex;
        public int playerGold;
        public List<int> unlockedUpgradeIDs;
        public List<Player_Upgrade> playerUpgrades;
    }
}
