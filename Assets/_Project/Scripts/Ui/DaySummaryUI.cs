using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    public class DaySummaryUI : MonoBehaviour
    {
        [Header("Header & Badge")]
        [SerializeField] private TextMeshProUGUI txtCurrentDayBadge;

        [Header("Today Summary Texts")]
        [SerializeField] private TextMeshProUGUI txtJobsCompleted;
        [SerializeField] private TextMeshProUGUI txtDailyRevenue;
        [SerializeField] private TextMeshProUGUI txtAverageRating;
        [SerializeField] private Image imgUsedTimeFill;

        [Header("Business Report Table")]
        [SerializeField] private Transform historyContainer;
        [SerializeField] private DayHistoryRowUI historyRowPrefab;

        [Header("Buttons")]
        [SerializeField] private Button btnBusinessReport;
        [SerializeField] private Button btnUpgradeShop;
        [SerializeField] private Button btnStartNextDay;

        [Header("Dữ liệu Test thử trong Play Mode")]
        [SerializeField] private bool testInPlayMode = true;
        [SerializeField] private int testCurrentDay = 3;
        [SerializeField] private int testJobs = 4;
        [SerializeField] private int testRevenue = 420;
        [SerializeField] private float testRating = 4.2f;
        [SerializeField] [Range(0f, 1f)] private float testUsedTimeNormalized = 0.85f;

        public event Action OnStartNextDayClicked;
        public event Action OnBusinessReportClicked;
        public event Action OnUpgradeShopClicked;

        private readonly List<DayHistoryRowUI> spawnedRows = new List<DayHistoryRowUI>();

        private void Awake()
        {
            if (btnStartNextDay != null)
                btnStartNextDay.onClick.AddListener(() => OnStartNextDayClicked?.Invoke());

            if (btnBusinessReport != null)
                btnBusinessReport.onClick.AddListener(() => OnBusinessReportClicked?.Invoke());

            if (btnUpgradeShop != null)
                btnUpgradeShop.onClick.AddListener(() => OnUpgradeShopClicked?.Invoke());
        }

        private void Start()
        {
            if (testInPlayMode)
            {
                var mockHistory = new List<DayHistoryRecord>
                {
                    new DayHistoryRecord(10, 11, 800, 4.9f),
                    new DayHistoryRecord(9, 10, 750, 4.8f),
                    new DayHistoryRecord(8, 9, 700, 4.7f),
                    new DayHistoryRecord(7, 8, 600, 4.7f),
                    new DayHistoryRecord(6, 7, 550, 4.6f),
                    new DayHistoryRecord(5, 6, 500, 4.5f),
                    new DayHistoryRecord(4, 5, 450, 4.3f),
                    new DayHistoryRecord(3, 4, 420, 4.2f),
                    new DayHistoryRecord(2, 3, 310, 4.0f),
                    new DayHistoryRecord(1, 2, 180, 3.5f)
                };

                DisplaySummary(testCurrentDay, testJobs, testRevenue, testRating, testUsedTimeNormalized, mockHistory);
            }
        }

        public void DisplaySummary(
            int currentDay,
            int jobsCompleted,
            int dailyRevenue,
            float averageRating,
            float usedTimePercent,
            List<DayHistoryRecord> previousDaysHistory = null)
        {
            if (txtCurrentDayBadge != null)
                txtCurrentDayBadge.text = $"Day {currentDay}";

            if (txtJobsCompleted != null)
                txtJobsCompleted.text = jobsCompleted.ToString();

            if (txtDailyRevenue != null)
                txtDailyRevenue.text = $"${dailyRevenue}";

            if (txtAverageRating != null)
                txtAverageRating.text = $"Star {averageRating:0.0}";

            if (imgUsedTimeFill != null)
                imgUsedTimeFill.fillAmount = Mathf.Clamp01(usedTimePercent);

            PopulateHistory(previousDaysHistory);

            gameObject.SetActive(true);
        }

        private void PopulateHistory(List<DayHistoryRecord> history)
        {
            if (historyContainer == null) return;

            // Xóa các dòng cũ đã sinh động
            foreach (var row in spawnedRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            spawnedRows.Clear();

            // Nếu không có dữ liệu truyền vào hoặc chưa gắn prefab thì giữ nguyên các dòng mẫu trong Scene
            if (history == null || history.Count == 0 || historyRowPrefab == null) return;

            // Xóa các con mẫu có sẵn trong container
            for (int i = historyContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(historyContainer.GetChild(i).gameObject);
            }

            // Sinh dòng mới từ danh sách lịch sử
            foreach (var record in history)
            {
                DayHistoryRowUI newRow = Instantiate(historyRowPrefab, historyContainer);
                newRow.SetData(record);
                spawnedRows.Add(newRow);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
