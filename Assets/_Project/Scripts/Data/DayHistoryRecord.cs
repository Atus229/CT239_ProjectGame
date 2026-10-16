using System;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [Serializable]
    public struct DayHistoryRecord
    {
        public int dayIndex;
        public int jobsCompleted;
        public int revenue;
        public float averageRating;
        public float usedTime;

        // Alias tương thích cho Core (DayManager)
        public int totalEarned
        {
            get => revenue;
            set => revenue = value;
        }
        public float averageStars
        {
            get => averageRating;
            set => averageRating = value;
        }
        
        public DayHistoryRecord(int dayIndex, int jobsCompleted, int revenue, float averageRating, float usedTime = 0f)
        {
            this.dayIndex = dayIndex;
            this.jobsCompleted = jobsCompleted;
            this.revenue = revenue;
            this.averageRating = averageRating;
            this.usedTime = usedTime;
        }
    }
}
