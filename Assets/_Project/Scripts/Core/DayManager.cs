using System;
using System.Collections.Generic;
using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Core
{
    [DefaultExecutionOrder(-100)]
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        [Header("Day Config & Runtime Data")] [SerializeField]
        private DayData currentDayData;
        private List<JobData> templateJobs = new List<JobData>(); // Lưu lại mẫu danh sách đơn ban đầu

        private float dayTimeBudget; // Quỹ thời gian tổng của ngày
        private float dayTimeRemaining; // Quỹ thời gian còn lại
        private int currentDayIndex;

        [Header("Daily Statistics")]
        private int completedJobsCount;
        private int totalEarnedInDay;
        private int totalStarsInDay;

        [Header("Player Economy")]
        [SerializeField] private int playerGold = 0; // Ví tiền tích luỹ của người chơi
        
        private float bonusTimeBudgetFromUpgrade = 0f; // Lưu trữ số giây nâng cấp tích lũy
        private bool isDayActive = false;              // Kiểm tra xem ngày có đang diễn ra hay đã kết thúc

        public float TimeRemaining => dayTimeRemaining; 
        public float TimeBudget => dayTimeBudget; 
        public int CurrentDayIndex => currentDayIndex;
        public int TotalEarnedInDay => totalEarnedInDay;
        public DayData CurrentDayData => currentDayData;
        public int PlayerGold
        {
            get => playerGold;
            set => playerGold = Mathf.Max(0, value);
        }

        private JobData currentActiveJob;
        private JobResult lastJobResult;
        public List<JobData> activeJobsOnBoard = new List<JobData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (currentDayData != null)
            {
                if (currentDayData.DailyJobs != null)
                {
                    templateJobs = new List<JobData>(currentDayData.DailyJobs);
                }
                StartNewDay(currentDayData);
            }
            else
            {
                DayData defaultDay = ScriptableObject.CreateInstance<DayData>();
                defaultDay.DayIndex = 1;
                defaultDay.DayTimeBudget = 180f;
                StartNewDay(defaultDay);
            }
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
            EventBus.Subscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(HandleContinueFromJobResult);
            EventBus.Subscribe<GameEvent.NextDayRequestedEvent>(HandleNextDayRequested);
            EventBus.Subscribe<GameEvent.UpgradePurchasedEvent>(HandleUpgradePurchased);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
            EventBus.Unsubscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(HandleContinueFromJobResult);
            EventBus.Unsubscribe<GameEvent.NextDayRequestedEvent>(HandleNextDayRequested);
            EventBus.Unsubscribe<GameEvent.UpgradePurchasedEvent>(HandleUpgradePurchased);
        }
        
        /// <summary>
        /// Bắt đầu một ngày làm việc mới
        /// </summary>
        public void StartNewDay(DayData dayData)
        {
            currentDayData = dayData;
            currentDayIndex = dayData.DayIndex;
            
            dayTimeBudget = dayData.DayTimeBudget + bonusTimeBudgetFromUpgrade; 
            dayTimeRemaining = dayTimeBudget;

            // Nếu dayData chưa có đơn hàng, nạp từ templateJobs ban đầu để người chơi luôn có đơn chơi ngày mới
            if (dayData.DailyJobs == null || dayData.DailyJobs.Count == 0)
            {
                activeJobsOnBoard = new List<JobData>(templateJobs);
            }
            else
            {
                activeJobsOnBoard = new List<JobData>(dayData.DailyJobs);
            }

            completedJobsCount = 0;
            totalEarnedInDay = 0;
            totalStarsInDay = 0;
            isDayActive = true;
            
            EventBus.Publish(new GameEvent.DayStartedEvent(currentDayIndex, dayTimeBudget, activeJobsOnBoard));
            EventBus.Publish(new GameEvent.DayTimeUpdatedEvent(dayTimeRemaining, dayTimeBudget));
        }

        /// <summary>
        /// Kiểm tra xem một đơn hàng có thể nhận hay không
        /// Điều kiện: Day Time Remaining >= Tmin của đơn
        /// </summary>
        public bool CanAcceptJob(JobData job)
        {
            if (job == null) return false;
            return dayTimeRemaining >= job.TimeMin; 
        }

        /// <summary>
        /// Trừ quỹ thời gian sau khi hoàn tất một đơn
        /// </summary>
        public void DeductTimeAndRecordJob(float totalJobTime, int earnedMoney, int stars)
        {
            dayTimeRemaining = Mathf.Max(0f, dayTimeRemaining - totalJobTime);

            completedJobsCount++;
            totalEarnedInDay += earnedMoney;
            playerGold += earnedMoney; // Tích lũy tiền vào ví người chơi
            totalStarsInDay += stars;
            
            EventBus.Publish(new GameEvent.DayTimeUpdatedEvent(dayTimeRemaining, dayTimeBudget));
        }

        /// <summary>
        /// Kiểm tra điều kiện tự động kết thúc ngày khi không còn đơn nào đủ điều kiện
        /// </summary>
        public void CheckAutoEndDayCondition(List<JobData> activeJobsOnBoard)
        {
            if (activeJobsOnBoard == null || activeJobsOnBoard.Count == 0 || dayTimeRemaining <= 0)
            {
                EndDay(isManual:false);
                return;
            }
            
            bool hasValidJob = false;
            foreach (var job in activeJobsOnBoard)
            {
                if (CanAcceptJob(job)) 
                {
                    hasValidJob = true;
                    break;
                }
            }

            // Nếu không còn đơn nào đủ điều kiện nhận -> Tự động kết thúc ngày
            if (hasValidJob)
            {
                EventBus.Publish(new GameEvent.DayTimeUpdatedEvent(dayTimeRemaining, dayTimeBudget));
            }
            else
            {
                EndDay(isManual: false);
            }
        }

        /// <summary>
        /// Kết thúc ngày làm việc
        /// </summary>
        public void EndDay(bool isManual = false)
        {
            isDayActive = false;
            
            float avgStars = completedJobsCount > 0 ? (float)totalStarsInDay / completedJobsCount : 0f;

            DayHistoryRecord record = new DayHistoryRecord
            {
                dayIndex = currentDayIndex,
                jobsCompleted = completedJobsCount,
                totalEarned = totalEarnedInDay,
                averageStars = avgStars,
                usedTime = dayTimeBudget - Mathf.Max(0, dayTimeRemaining)
            };

            // Phát tin sự kiện kết thúc ngày
            EventBus.Publish(new GameEvent.DayEndedEvent(record));
        }
        
        private void HandleJobSubmitted(GameEvent.JobSubmittedEvent e)
        {
            DeductTimeAndRecordJob(e.JobResult.TotalJobTime, e.JobResult.EarnedGold, e.JobResult.Stars);
        }
        
        private void HandleNextDayRequested(GameEvent.NextDayRequestedEvent obj)
        {
            // Tăng chỉ số ngày và bắt đầu ngày mới với danh sách đơn hàng mẫu
            DayData nextDay = ScriptableObject.CreateInstance<DayData>();
            nextDay.DayIndex = currentDayIndex + 1;
            nextDay.DayTimeBudget = 180f; // Quỹ cơ bản (sẽ được tự động cộng thêm bonusTimeBudgetFromUpgrade trong StartNewDay)
            nextDay.DailyJobs = new List<JobData>(templateJobs);
            StartNewDay(nextDay);
        }

        private void HandleContinueFromJobResult(GameEvent.ContinueFromJobResultEvent obj)
        {
            // Xóa đơn ra khỏi bảng việc
            if (currentActiveJob != null)
            {
                activeJobsOnBoard.Remove(currentActiveJob);
                currentActiveJob = null;
            }
            
            // Kiểm tra điều kiện kết thúc ngày
            CheckAutoEndDayCondition(activeJobsOnBoard);
        }

        private void HandleUpgradePurchased(GameEvent.UpgradePurchasedEvent e)
        {
            if (e.UpgradeType == UpgradeData.TypeDayTimeBudget)
            {
                IncreaseDayBudget(30f);
            }
        }

        private void HandleJobSelected(GameEvent.JobSelectedEvent e)
        {
            currentActiveJob = e.SelectedJob;
        }

        [ContextMenu("TEST: Tăng 20s Quỹ Giờ")]
        public void TestIncreaseBudget() => IncreaseDayBudget(20f);
        
        /// <summary>
        /// Cộng thêm một lượng thời gian (tính bằng giây) vào quỹ thời gian làm việc trong ngày
        /// Tự động cập nhật TimeBudget, TimeRemaining và phát sự kiện cập nhật UI
        /// API dành cho Leader gọi khi áp dụng Nâng cấp Quỹ thời gian
        /// </summary>
        /// <param name="extraSeconds">Số giây muốn cộng thêm (ví dụ: 20f để tăng từ 180s lên 200s)</param>
        public void IncreaseDayBudget(float extraSeconds)
        {
            if (extraSeconds <= 0f) return;

            bonusTimeBudgetFromUpgrade += extraSeconds;

            if (isDayActive)
            {
                dayTimeBudget += extraSeconds;
                dayTimeRemaining += extraSeconds;

                // Cập nhật giao diện thanh quỹ thời gian theo thời gian thực
                EventBus.Publish(new GameEvent.DayTimeUpdatedEvent(dayTimeRemaining, dayTimeBudget));
            }

            Debug.Log($"[DayManager] Đã cộng +{extraSeconds}s nâng cấp. Quỹ hiện tại: {dayTimeBudget}s (Bonus tích luỹ: +{bonusTimeBudgetFromUpgrade}s).");
        }
    }
}
