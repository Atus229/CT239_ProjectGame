using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "DayData", menuName = "Scriptable Objects/DayData")]
    public class DayData : ScriptableObject
    {
        [Header("Thông tin ngày")] [SerializeField]
        private int dayIndex = 1;
        
        public int DayIndex
        {
            get => dayIndex;
            set => dayIndex = value;
        }

        [Header("Quỹ thời gian(phút)")] [SerializeField] [Min(10f)]
        private float dayTimeBudget = 180f;
        public float DayTimeBudget
        {
            get => dayTimeBudget;
            set => dayTimeBudget = value;
        }

        [Header("Danh sach don hang trong ngay")] [SerializeField]
        private List<JobData> dailyJobs = new List<JobData>();

        public List<JobData> DailyJobs => dailyJobs;
        
        

    }
    
}
