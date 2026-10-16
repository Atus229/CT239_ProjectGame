using System;
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
    }
    
}
