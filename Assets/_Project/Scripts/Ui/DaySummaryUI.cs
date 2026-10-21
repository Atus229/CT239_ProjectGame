using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
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
        [SerializeField] private bool testInPlayMode = false;
        [SerializeField] private int testCurrentDay = 3;
        [SerializeField] private int testJobs = 4;
        [SerializeField] private int testRevenue = 420;
        [SerializeField] private float testRating = 4.2f;
        [SerializeField] [Range(0f, 1f)] private float testUsedTimeNormalized = 0.85f;

        public event Action OnStartNextDayClicked;
        public event Action OnBusinessReportClicked;
        public event Action OnUpgradeShopClicked;

        public bool TestInPlayMode
        {
            get => testInPlayMode;
            set => testInPlayMode = value;
        }

        public void SetTestMode(bool enabled)
        {
            testInPlayMode = enabled;
        }

        private readonly List<DayHistoryRowUI> spawnedRows = new List<DayHistoryRowUI>();

        private void Awake()
        {
            if (btnStartNextDay != null)
                btnStartNextDay.onClick.AddListener(()=>
            {
                gameObject.SetActive(false);
                EventBus.Publish(new GameEvent.NextDayRequestedEvent());
                OnStartNextDayClicked?.Invoke();
            });

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

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.DayEndedEvent>(HandleDayEnded); 
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.DayEndedEvent>(HandleDayEnded); 
        }

        public void DisplaySummary(
            int currentDay,
            int jobsCompleted,
            int dailyRevenue,
            float averageRating,
            float usedTimeNormalized,
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
                imgUsedTimeFill.fillAmount = Mathf.Clamp01(usedTimeNormalized);

            PopulateHistory(previousDaysHistory);

            gameObject.SetActive(true);
        }

        private void PopulateHistory(List<DayHistoryRecord> history)
        {
            if (historyContainer == null) return;

            // Nếu không có dữ liệu truyền vào hoặc chưa gắn prefab thì giữ nguyên
            if (history == null || history.Count == 0 || historyRowPrefab == null) return;

            // 1. Dọn sạch mọi dòng cũ
            for (int i = historyContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(historyContainer.GetChild(i).gameObject);
            }

            // 2. Sinh các dòng mới
            foreach (var record in history)
            {
                DayHistoryRowUI newRow = Instantiate(historyRowPrefab, historyContainer);
                newRow.SetData(record);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        private void HandleDayEnded(GameEvent.DayEndedEvent e)
        {
            DayHistoryRecord record = e.record;
            
            // Điền các chỉ số text
            if (txtCurrentDayBadge != null)
                txtCurrentDayBadge.text = $"Day {record.dayIndex}";
            
            if (txtJobsCompleted != null)
                txtJobsCompleted.text = record.jobsCompleted.ToString();
            
            if (txtDailyRevenue != null)
                txtDailyRevenue.text = $"${record.totalEarned}";
            
            if (txtAverageRating != null)
                txtAverageRating.text = $"Star {record.averageStars:0.0}";
            
            // Tính toán thanh thời gian đã dùng (tránh lỗi chia cho 0)
            if (imgUsedTimeFill != null)
            {
                float totalBudget = (DayManager.Instance != null && DayManager.Instance.TimeBudget > 0)
                    ? DayManager.Instance.TimeBudget
                    : 180f;
                imgUsedTimeFill.fillAmount = Mathf.Clamp01(record.usedTime / totalBudget);
            }
            // Nạp danh sách lịch sử bảng việc (nếu có)
            PopulateHistory(null);
            
            // Bật sáng bảng tổng kết lên màn hình
            gameObject.SetActive(true);
        }
    }
}
