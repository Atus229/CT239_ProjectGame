using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Project.Scripts.Data;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Ui
{
    public class DaySummaryUI : MonoBehaviour
    {
        public static DaySummaryUI Instance { get; private set; }
        private bool openedFromJobBoard = false;
        
        [Header("Header & Badge")] [SerializeField]
        private TextMeshProUGUI txtCurrentDayBadge;

        [SerializeField] private TextMeshProUGUI txtHeaderTitle;

        [Header("Today Summary Texts")] [SerializeField]
        private TextMeshProUGUI txtJobsCompleted;

        [SerializeField] private TextMeshProUGUI txtDailyRevenue;
        [SerializeField] private TextMeshProUGUI txtAverageRating;
        [SerializeField] private Image imgUsedTimeFill;

        [Header("Business Report Table")] [SerializeField]
        private Transform historyContainer;

        [SerializeField] private DayHistoryRowUI historyRowPrefab;

        [Header("Buttons")] [SerializeField] private Button btnBusinessReport;
        [SerializeField] private Button btnUpgradeShop;
        [SerializeField] private Button btnStartNextDay;

        [Header("Day 7 Victory / Defeat Display")] [SerializeField]
        private TextMeshProUGUI txt_SummaryDay7;

        [SerializeField] private TextMeshProUGUI txt_ContainerSummaryD7;

        [Header("Switch Views & Chart")] [SerializeField]
        private GameObject imgTruckIllustration; // Kéo Img_TruckIllustration vào

        [SerializeField] private GameObject todaySummaryBox; // Kéo TodaySummaryBox vào
        [SerializeField] private GameObject businessReportBox; // Kéo BusinessReportBox vào
        [SerializeField] private RectTransform chartContainer; // Khung chứa biểu đồ cột

        private bool isShowingReportView = false;

        [Header("Dữ liệu Test thử trong Play Mode")] [SerializeField]
        private bool testInPlayMode = false;

        [SerializeField] private int testCurrentDay = 3;
        [SerializeField] private int testJobs = 4;
        [SerializeField] private int testRevenue = 420;
        [SerializeField] private float testRating = 4.2f;
        [SerializeField] [Range(0f, 1f)] private float testUsedTimeNormalized = 0.85f;

        [SerializeField] CanvasGroup canvasGroup;

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
            Instance = this;
            
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            SetVisibility(false);
            SetDay7SummaryActive(false);

            if (btnStartNextDay != null)
                btnStartNextDay.onClick.AddListener(() =>
                {
                    SetVisibility(false);
                    EventBus.Publish(new GameEvent.NextDayRequestedEvent());
                    OnStartNextDayClicked?.Invoke();
                });

            if (btnBusinessReport != null)
            {
                if (btnBusinessReport != null)
                {
                    btnBusinessReport.onClick.RemoveAllListeners();
                    btnBusinessReport.onClick.AddListener(HandleBusinessReportButtonClicked);
                }
            }

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

            SetVisibility(true);
            transform.SetAsLastSibling();
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
            SetVisibility(false);
        }

        private void HandleDayEnded(GameEvent.DayEndedEvent e)
        {
            isShowingReportView = false;
            openedFromJobBoard = false;
            if (btnStartNextDay != null) btnStartNextDay.gameObject.SetActive(true);
            
            
            // Mặc định tắt dòng tổng tiền Ngày 7, chỉ khi tới Ngày 7 mới bật lên
            SetDay7SummaryActive(false);
            DayHistoryRecord record = e.record;

            // Điền các chỉ số text
            if (txtCurrentDayBadge != null)
                txtCurrentDayBadge.text = $"Ngày {record.dayIndex}";

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
            if (DayManager.Instance != null)
            {
                PopulateHistory(DayManager.Instance.AllDaysHistory);
            }

            // Bật sáng bảng tổng kết lên màn hình
            SetVisibility(true);
            transform.SetAsLastSibling();

            if (record.dayIndex >= 7)
            {
                if (imgTruckIllustration != null) imgTruckIllustration.SetActive(true);
                if (todaySummaryBox != null) todaySummaryBox.SetActive(true);
                if (businessReportBox != null) businessReportBox.SetActive(true);
                if (chartContainer != null) chartContainer.gameObject.SetActive(false);
                // Lấy tiền từ ví thực tế (hoặc từ record nếu test)
                int totalGold = (DayManager.Instance != null && DayManager.Instance.PlayerGold > 0)
                    ? DayManager.Instance.PlayerGold
                    : record.totalEarned;
                bool isVictory = totalGold >= 5000;
                // 1. Ô Badge góc phải: GIỮ NGUYÊN (Day 7), không đổi chữ để không bị vỡ khung
                if (txtCurrentDayBadge != null)
                {
                    txtCurrentDayBadge.text = $"Ngày {record.dayIndex}";
                }

                // 2. Chữ "Day Summary" góc trên trái: GẠCH CHÂN hiển thị THẮNG hoặc THUA
                if (txtHeaderTitle != null)
                {
                    if (isVictory)
                    {
                        txtHeaderTitle.text = "<u>CHIẾN THẮNG!</u>";
                    }
                    else
                    {
                        txtHeaderTitle.text = "<u>THẤT BẠI!</u>";
                    }
                }

                // 3. TẮT 2 nút Báo cáo và Shop nâng cấp
                if (btnBusinessReport != null) btnBusinessReport.gameObject.SetActive(false);
                if (btnUpgradeShop != null) btnUpgradeShop.gameObject.SetActive(false);
                // 4. BẬT Text mới "txt_SummaryDay7" tại vị trí 2 nút vừa tắt
                SetDay7SummaryActive(true);
                if (txt_SummaryDay7 != null)
                {
                    txt_SummaryDay7.text = $"${totalGold} / $5000";
                }

                // 5. Cập nhật nút bấm dưới cùng: Về Menu hoặc Chơi lại
                if (btnStartNextDay != null)
                {
                    TextMeshProUGUI btnText = btnStartNextDay.GetComponentInChildren<TextMeshProUGUI>();
                    btnStartNextDay.onClick.RemoveAllListeners();
                    if (isVictory)
                    {
                        if (btnText != null) btnText.text = "VỀ MENU CHÍNH";
                        btnStartNextDay.onClick.AddListener(() =>
                        {
                            _Project.Scripts.Managers.SaveLoadManager.DeleteSaveData();
                            SceneManager.LoadScene("MainMenu");
                        });
                    }
                    else
                    {
                        if (btnText != null) btnText.text = "CHƠI LẠI TỪ ĐẦU";
                        btnStartNextDay.onClick.AddListener(() =>
                        {
                            _Project.Scripts.Managers.SaveLoadManager.DeleteSaveData();
                            SceneManager.LoadScene("MainMenu");
                        });
                    }
                }
            }
            else
            {
                // NẾU LÀ NGÀY BÌNH THƯỜNG (Ngày 1 -> 6): Bật lại các nút và ẩn text ngày 7
                SetDay7SummaryActive(false);
                if (btnBusinessReport != null) btnBusinessReport.gameObject.SetActive(true);
                if (btnUpgradeShop != null) btnUpgradeShop.gameObject.SetActive(true);

                ApplyViewState();
            }
        }

        private void SetVisibility(bool isVisible)
        {
            if (isVisible && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
            else
            {
                gameObject.SetActive(isVisible);
            }

            if (isVisible)
            {
                transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Chuyển đổi qua lại giữa màn hình tóm tắt hôm nay và màn hình báo cáo kinh doanh + biểu đồ
        /// </summary>
        public void ToggleBusinessReportView()
        {
            isShowingReportView = !isShowingReportView;
            ApplyViewState();
        }

        private void ApplyViewState()
        {
            // Nếu đang bật xem Báo Cáo
            if (isShowingReportView)
            {
                
                if (todaySummaryBox != null) todaySummaryBox.SetActive(false);

                if (businessReportBox != null) businessReportBox.SetActive(true);
                if (chartContainer != null)
                {
                    chartContainer.gameObject.SetActive(true);
                    DrawRevenueChart(); // Vẽ biểu đồ cột
                }

                if (btnBusinessReport != null)
                {
                    var txtBtn = btnBusinessReport.GetComponentInChildren<TextMeshProUGUI>();
                    if (txtBtn != null) txtBtn.text = "Quay Lại";
                }
            }
            else // Trạng thái xem tóm tắt hôm nay (Mặc định)
            {
                if (imgTruckIllustration != null) imgTruckIllustration.SetActive(true);
                if (todaySummaryBox != null) todaySummaryBox.SetActive(true);

                if (businessReportBox != null) businessReportBox.SetActive(false);
                if (chartContainer != null) chartContainer.gameObject.SetActive(false);

                if (btnBusinessReport != null)
                {
                    var txtBtn = btnBusinessReport.GetComponentInChildren<TextMeshProUGUI>();
                    if (txtBtn != null) txtBtn.text = "Báo Cáo";
                }
            }
        }

        private void DrawRevenueChart()
{
    if (chartContainer == null) return;

    // 1. Xoá các cột cũ
    for (int i = chartContainer.childCount - 1; i >= 0; i--)
    {
        Destroy(chartContainer.GetChild(i).gameObject);
    }

    // 2. Lấy danh sách lịch sử
    List<DayHistoryRecord> history = DayManager.Instance != null ? DayManager.Instance.AllDaysHistory : null;
    
    // NẾU CHƯA CÓ LỊCH SỬ HOẶC VỪA TUA F3 CHƯA CÓ DOANH THU (0$):
    // Tự sinh 3 ngày mẫu để xem thử
    bool hasValidRevenue = false;
    if (history != null && history.Count > 0)
    {
        foreach (var r in history)
        {
            if (r.revenue > 0) { hasValidRevenue = true; break; }
        }
    }

    if (!hasValidRevenue)
    {
        history = new List<DayHistoryRecord>
        {
            new DayHistoryRecord(1, 2, 220, 4.5f),
            new DayHistoryRecord(2, 3, 480, 4.8f),
            new DayHistoryRecord(3, 4, 750, 5.0f)
        };
    }

    // 3. Tìm mốc doanh thu cao nhất để cân đối chiều cao
    int maxRevenue = 600;
    foreach (var rec in history)
    {
        if (rec.revenue > maxRevenue) maxRevenue = rec.revenue;
    }

    // TĂNG CHIỀU CAO CỘT LÊN 280px (gấp đôi cũ để lấp đầy khoảng trống)
    float maxHeight = 280f; 

    // MÃ MÀU THEO YÊU CẦU: #94493F (Nâu đỏ retro)
    Color32 themeColor = new Color32(148, 73, 63, 255); 
    Color32 bgSlotColor = new Color32(148, 73, 63, 35); // Khung nền mờ tiệp màu

    // 4. Vẽ từng cột
    foreach (var rec in history)
    {
        // Container của 1 cột - TĂNG RỘNG LÊN 90px
        GameObject colObj = new GameObject($"Col_Day{rec.dayIndex}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        colObj.transform.SetParent(chartContainer, false);
        
        LayoutElement le = colObj.GetComponent<LayoutElement>();
        le.preferredWidth = 90f; // Bự ra 90px
        le.minWidth = 80f;
        le.preferredHeight = maxHeight + 60f;

        Image imgBg = colObj.GetComponent<Image>();
        imgBg.color = bgSlotColor; // Nền mờ tiệp màu tông ấm

        // Thanh dâng (Bar Fill)
        GameObject barFill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(colObj.transform, false);
        RectTransform rtFill = barFill.GetComponent<RectTransform>();
        rtFill.anchorMin = new Vector2(0.12f, 0f);
        rtFill.anchorMax = new Vector2(0.88f, 0f);
        rtFill.pivot = new Vector2(0.5f, 0f);
        rtFill.anchoredPosition = new Vector2(0, 32f); // Cách đáy 32px để nhường chỗ cho chữ ngày

        // Chiều cao thanh màu theo doanh thu
        float h = Mathf.Max(25f, ((float)rec.revenue / maxRevenue) * maxHeight);
        rtFill.sizeDelta = new Vector2(0, h);

        Image imgFill = barFill.GetComponent<Image>();
        imgFill.color = themeColor; // MÀU #94493F

        // Text số tiền trên đỉnh cột (Ví dụ: $480)
        GameObject txtValObj = new GameObject("TxtVal", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtValObj.transform.SetParent(colObj.transform, false);
        RectTransform rtVal = txtValObj.GetComponent<RectTransform>();
        rtVal.anchorMin = new Vector2(0.5f, 0f);
        rtVal.anchorMax = new Vector2(0.5f, 0f);
        rtVal.pivot = new Vector2(0.5f, 0.5f);
        rtVal.sizeDelta = new Vector2(100f, 30f);
        rtVal.anchoredPosition = new Vector2(0, 32f + h + 18f);
        
        TextMeshProUGUI txtVal = txtValObj.GetComponent<TextMeshProUGUI>();
        if (txtDailyRevenue != null) txtVal.font = txtDailyRevenue.font;
        txtVal.text = $"${rec.revenue}";
        txtVal.fontSize = 16; // Chữ số to rõ hơn
        txtVal.fontStyle = FontStyles.Bold;
        txtVal.alignment = TextAlignmentOptions.Center;
        txtVal.color = themeColor; // Đồng bộ màu nâu đỏ

        // Text nhãn ngày dưới chân cột (Ví dụ: Ngày 1, Ngày 2...)
        GameObject txtDayObj = new GameObject("TxtDay", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtDayObj.transform.SetParent(colObj.transform, false);
        RectTransform rtDay = txtDayObj.GetComponent<RectTransform>();
        rtDay.anchorMin = new Vector2(0.5f, 0f);
        rtDay.anchorMax = new Vector2(0.5f, 0f);
        rtDay.pivot = new Vector2(0.5f, 0.5f);
        rtDay.sizeDelta = new Vector2(90f, 26f);
        rtDay.anchoredPosition = new Vector2(0, 16f);

        TextMeshProUGUI txtDay = txtDayObj.GetComponent<TextMeshProUGUI>();
        if (txtDailyRevenue != null) txtDay.font = txtDailyRevenue.font;
        txtDay.text = $"Ngày {rec.dayIndex}";
        txtDay.fontSize = 14;
        txtDay.fontStyle = FontStyles.Bold;
        txtDay.alignment = TextAlignmentOptions.Center;
        txtDay.color = new Color32(80, 40, 35, 255); // Chữ màu sẫm rõ ràng
    }
}
        
        private void HandleBusinessReportButtonClicked()
        {
            if (openedFromJobBoard)
            {
                // Nếu mở từ Danh sách công việc -> Bấm nút này sẽ đóng bảng báo cáo, trở về bảng việc
                openedFromJobBoard = false;
                SetVisibility(false);
            }
            else
            {
                // Nếu là cuối ngày bình thường -> Chuyển đổi giữa Tóm tắt ngày và Báo cáo
                ToggleBusinessReportView();
            }
        }

        /// <summary>
        /// Mở trực tiếp chế độ xem Báo Cáo Kinh Doanh khi người chơi bấm nút trên JobBoard
        /// </summary>
        public void OpenAsReportOnly()
        {
            openedFromJobBoard = true;
            isShowingReportView = true;

            // 1. Nạp danh sách lịch sử các ngày
            if (DayManager.Instance != null)
            {
                PopulateHistory(DayManager.Instance.AllDaysHistory);
            }

            // 2. Cập nhật Badge ngày hiện tại
            if (txtCurrentDayBadge != null && DayManager.Instance != null)
            {
                txtCurrentDayBadge.text = $"Ngày {DayManager.Instance.CurrentDayIndex}";
            }

            // 3. Tạm ẩn nút Sang ngày mới, Shop & Tổng tài sản Ngày 7 (vì đang ở giữa ngày)
            if (btnStartNextDay != null) btnStartNextDay.gameObject.SetActive(false);
            if (btnUpgradeShop != null) btnUpgradeShop.gameObject.SetActive(false);
            SetDay7SummaryActive(false);

            // 4. Bật khung Báo cáo + Vẽ biểu đồ
            if (todaySummaryBox != null) todaySummaryBox.SetActive(false);
            if (businessReportBox != null) businessReportBox.SetActive(true);
            if (chartContainer != null)
            {
                chartContainer.gameObject.SetActive(true);
                DrawRevenueChart();
            }

            // 5. Đổi nút thành "Quay Lại"
            if (btnBusinessReport != null)
            {
                btnBusinessReport.gameObject.SetActive(true);
                var txtBtn = btnBusinessReport.GetComponentInChildren<TextMeshProUGUI>();
                if (txtBtn != null) txtBtn.text = "Quay Lại";
            }

            // 6. Hiện bảng lên trên cùng
            SetVisibility(true);
            transform.SetAsLastSibling();
        }

        /// <summary>
        /// Bật/Tắt đồng bộ cụm hiển thị Tổng tài sản Ngày 7 (tắt cả chữ tiêu đề lẫn con số)
        /// </summary>
        private void SetDay7SummaryActive(bool active)
        {
            if (txt_SummaryDay7 != null)
            {
                // Nếu txt_SummaryDay7 nằm trong một container/GameObject cha (ví dụ Txt_SummaryDay7 chứa chữ "Tổng tài sản: ")
                if (txt_SummaryDay7.transform.parent != null &&
                    txt_SummaryDay7.transform.parent.name.Contains("SummaryDay7"))
                {
                    txt_SummaryDay7.transform.parent.gameObject.SetActive(active);
                }
                txt_SummaryDay7.gameObject.SetActive(active);
            }

            if (txt_ContainerSummaryD7 != null)
            {
                txt_ContainerSummaryD7.gameObject.SetActive(active);
            }
        }


#if UNITY_EDITOR
        [ContextMenu("TEST: Giả lập CHIẾN THẮNG Ngày 7 (5.200$)")]
        public void TestVictoryDay7()
        {
            // Gán trực tiếp 5200$ vào DayManager để test đúng điều kiện
            if (DayManager.Instance != null) DayManager.Instance.PlayerGold = 5200;

            DayHistoryRecord mock = new DayHistoryRecord(7, 3, 5200, 4.8f);
            HandleDayEnded(new GameEvent.DayEndedEvent(mock));
        }

        [ContextMenu("TEST: Giả lập THẤT BẠI Ngày 7 (3.800$)")]
        public void TestDefeatDay7()
        {
            // Gán 3800$ vào DayManager
            if (DayManager.Instance != null) DayManager.Instance.PlayerGold = 3800;

            DayHistoryRecord mock = new DayHistoryRecord(7, 2, 3800, 3.5f);
            HandleDayEnded(new GameEvent.DayEndedEvent(mock));
        }
#endif
    }
}