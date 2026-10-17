using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    public class PackingResultCoordinator : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private OrderResultUI orderResultUI;
        [SerializeField] private DaySummaryUI daySummaryUI;

        [Header("Test Options")]
        [Tooltip("Nếu tick, bấm 'Tiếp tục' trên OrderResult sẽ lập tức mở DaySummaryUI để test nhanh.")]
        [SerializeField] private bool testForceEndDayOnContinue = false;

        [Tooltip("Tự động kết thúc ngày khi quỹ thời gian còn lại không đủ nhận thêm đơn (TimeRemaining < Tmin)")]
        [SerializeField] private bool autoEndDayWhenNoTime = true;

        private JobResult lastJobResult;
        private JobData lastJobData;

        private void Awake()
        {
            // Tắt cờ mock test data và ẩn panel ban đầu
            if (orderResultUI != null)
            {
                orderResultUI.SetTestMode(false);
                orderResultUI.Close();
            }

            if (daySummaryUI != null)
            {
                daySummaryUI.SetTestMode(false);
                daySummaryUI.Close();
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Subscribe<GameEvent.DayEndedEvent>(HandleDayEnded);

            if (orderResultUI != null)
            {
                orderResultUI.OnContinueClicked += HandleContinueFromOrderResult;
            }

            if (daySummaryUI != null)
            {
                daySummaryUI.OnStartNextDayClicked += HandleStartNextDay;
            }
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Unsubscribe<GameEvent.DayEndedEvent>(HandleDayEnded);

            if (orderResultUI != null)
            {
                orderResultUI.OnContinueClicked -= HandleContinueFromOrderResult;
            }

            if (daySummaryUI != null)
            {
                daySummaryUI.OnStartNextDayClicked -= HandleStartNextDay;
            }
        }

        /// <summary>
        /// Xử lý khi người chơi bấm nút Xác nhận vận chuyển
        /// </summary>
        private void HandleJobSubmitted(GameEvent.JobSubmittedEvent e)
        {
            if (orderResultUI == null)
            {
                Debug.LogWarning("[PackingResultCoordinator] Chưa gán OrderResultUI trong Inspector!");
                return;
            }

            lastJobResult = e.JobResult;
            lastJobData = e.Job;

            // 1. Lấy thông số khung thời gian mục tiêu
            float tMin = 40f;
            float tMax = 60f;
            if (JobEconomyManager.Instance != null)
            {
                tMin = JobEconomyManager.Instance.TimeMin;
                tMax = JobEconomyManager.Instance.TimeMax;
            }

            // 2. Lấy thông số dung tích thùng xe từ GridManager
            float usedCapacity = 0f;
            float maxCapacity = 16f;
            if (GridManager.Instance != null)
            {
                usedCapacity = GridManager.Instance.OccupiedCellCount;
                maxCapacity = GridManager.Instance.TotalCellCount;
            }

            // 3. Lấy quỹ thời gian ngày còn lại từ DayManager (nếu có)
            float dayTimeRemaining = 0f;
            if (DayManager.Instance != null)
            {
                dayTimeRemaining = DayManager.Instance.TimeRemaining;
            }

            // 4. Sinh lời nhận xét của khách hàng dựa trên kết quả
            string quote = GetCustomerQuote(e.JobResult.Stars, e.JobResult.IsOnTime);

            // 5. Hiển thị OrderResultUI
            orderResultUI.DisplayResult(
                stars: e.JobResult.Stars,
                totalJobTime: e.JobResult.TotalJobTime,
                targetMin: tMin,
                targetMax: tMax,
                moneyEarned: e.JobResult.EarnedGold,
                usedCapacity: usedCapacity,
                maxCapacity: maxCapacity,
                dayTimeRemaining: dayTimeRemaining,
                reviewQuote: quote
            );
        }

        /// <summary>
        /// Xử lý khi bấm nút Tiếp tục trên OrderResultUI
        /// </summary>
        private void HandleContinueFromOrderResult()
        {
            if (orderResultUI != null)
            {
                orderResultUI.Close();
            }

            // Nếu người chơi bật cờ test ép buộc xem DaySummary
            if (testForceEndDayOnContinue)
            {
                TriggerEndDay();
                return;
            }

            // Nếu có DayManager trong Scene, kiểm tra điều kiện kết thúc ngày
            if (DayManager.Instance != null && autoEndDayWhenNoTime)
            {
                float tMin = JobEconomyManager.Instance != null ? JobEconomyManager.Instance.TimeMin : 40f;
                if (DayManager.Instance.TimeRemaining < tMin)
                {
                    Debug.Log("<color=#FFA500>[PackingResultCoordinator] Quỹ thời gian ngày không đủ cho đơn tiếp theo -> Kết thúc ngày!</color>");
                    DayManager.Instance.EndDay(isManual: false);
                    return;
                }
            }

            Debug.Log("[PackingResultCoordinator] Đã đóng OrderResultUI. Quỹ ngày vẫn còn, sẵn sàng cho đơn tiếp theo.");
        }

        /// <summary>
        /// Xử lý khi ngày làm việc kết thúc
        /// </summary>
        private void HandleDayEnded(GameEvent.DayEndedEvent e)
        {
            if (daySummaryUI == null)
            {
                Debug.LogWarning("[PackingResultCoordinator] Chưa gán DaySummaryUI trong Inspector!");
                return;
            }

            float totalBudget = DayManager.Instance != null && DayManager.Instance.TimeBudget > 0
                ? DayManager.Instance.TimeBudget
                : 180f;

            float usedTimeNormalized = Mathf.Clamp01(e.record.usedTime / totalBudget);

            daySummaryUI.DisplaySummary(
                currentDay: e.record.dayIndex,
                jobsCompleted: e.record.jobsCompleted,
                dailyRevenue: e.record.totalEarned,
                averageRating: e.record.averageStars,
                usedTimeNormalized: usedTimeNormalized,
                previousDaysHistory: null
            );
        }

        /// <summary>
        /// Xử lý khi người chơi bấm nút Bắt đầu ngày mới trên DaySummaryUI
        /// </summary>
        private void HandleStartNextDay()
        {
            if (daySummaryUI != null)
            {
                daySummaryUI.Close();
            }

            Debug.Log("[PackingResultCoordinator] Bắt đầu ngày mới!");
            if (DayManager.Instance != null)
            {
                DayData nextDay = ScriptableObject.CreateInstance<DayData>();
                nextDay.DayIndex = DayManager.Instance.CurrentDayIndex + 1;
                nextDay.DayTimeBudget = DayManager.Instance.TimeBudget > 0 ? DayManager.Instance.TimeBudget : 180f;
                DayManager.Instance.StartNewDay(nextDay);
            }
        }

        private void TriggerEndDay()
        {
            if (DayManager.Instance != null)
            {
                DayManager.Instance.EndDay(isManual: true);
            }
            else
            {
                // Fallback nếu trong Scene chưa có DayManager: tạo mock record để test giao diện
                DayHistoryRecord mockRecord = new DayHistoryRecord
                {
                    dayIndex = 1,
                    jobsCompleted = 1,
                    totalEarned = lastJobResult.EarnedGold,
                    averageStars = lastJobResult.Stars,
                    usedTime = lastJobResult.TotalJobTime
                };
                EventBus.Publish(new GameEvent.DayEndedEvent(mockRecord));
            }
        }

        /// <summary>
        /// Sinh lời nhận xét khách hàng dựa trên đánh giá sao
        /// </summary>
        private string GetCustomerQuote(int stars, bool isOnTime)
        {
            if (stars >= 5)
            {
                return "Tuyệt vời! Đồ đạc được sắp xếp hoàn hảo và giao rất đúng giờ.";
            }

            if (stars == 4)
            {
                return "Hàng đến an toàn, xe gọn gàng. Cảm ơn bạn rất nhiều!";
            }

            if (stars == 3)
            {
                return "Giao hàng hơi trễ một chút, nhưng đồ đạc vẫn nguyên vẹn.";
            }

            if (stars == 2)
            {
                return "Giao trễ khá lâu khiến tôi phải chờ, cần cải thiện tốc độ!";
            }

            return "Quá trễ hẹn! Rất không hài lòng về thời gian giao hàng.";
        }
    }
}
