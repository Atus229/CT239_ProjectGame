using UnityEngine;
using TMPro;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    public class DayHistoryRowUI : MonoBehaviour
    {
        [Header("Text Components")]
        [SerializeField] private TextMeshProUGUI txtDay;
        [SerializeField] private TextMeshProUGUI txtJobs;
        [SerializeField] private TextMeshProUGUI txtRevenue;
        [SerializeField] private TextMeshProUGUI txtRating;

        public void SetData(DayHistoryRecord record)
        {
            if (txtDay != null) txtDay.text = $"Day {record.dayIndex}";
            if (txtJobs != null) txtJobs.text = record.jobsCompleted.ToString();
            if (txtRevenue != null) txtRevenue.text = $"${record.revenue}";
            if (txtRating != null) txtRating.text = record.averageRating.ToString("0.0");
        }

        public void SetData(int day, int jobs, int revenue, float rating)
        {
            SetData(new DayHistoryRecord(day, jobs, revenue, rating));
        }
    }
}
