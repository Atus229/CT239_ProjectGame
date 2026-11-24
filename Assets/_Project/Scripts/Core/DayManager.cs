using System;
using System.Collections.Generic;
using _Project.Scripts.Data;
using _Project.Scripts.Managers;
using UnityEngine;

namespace _Project.Scripts.Core
{
    [DefaultExecutionOrder(-100)]
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        [Header("Day Config & Runtime Data")] [SerializeField]
        private DayData currentDayData;
        [SerializeField] private List<DayData> scheduledDays = new List<DayData>();
        private List<JobData> templateJobs = new List<JobData>(); // Lưu lại mẫu danh sách đơn ban đầu

        private float dayTimeBudget; // Quỹ thời gian tổng của ngày
        private float dayTimeRemaining; // Quỹ thời gian còn lại
        private int currentDayIndex;

        [Header("Daily Statistics")]
        private int completedJobsCount;
        private int totalEarnedInDay;
        private int totalStarsInDay;
        
        [Header("Reputation Economy (BR-05)")]
        private int totalReputationStars = 0;
        public int TotalReputationStars => totalReputationStars;

        [Header("Player Economy")]
        [SerializeField] private int playerGold = 0; // Ví tiền tích luỹ của người chơi
        
        private float bonusTimeBudgetFromUpgrade = 0f; // Lưu trữ số giây nâng cấp tích lũy
        private bool isDayActive = false;              // Kiểm tra xem ngày có đang diễn ra hay đã kết thúc

        public float TimeRemaining => dayTimeRemaining; 
        public float TimeBudget => dayTimeBudget; 
        public int CurrentDayIndex => currentDayIndex;
        public bool CanSaveProgress => currentDayIndex > 0 && !isDayActive;
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
        
        //Danh sách lưu lịch sử các ngày
        private readonly List<DayHistoryRecord> allDaysHistory = new List<DayHistoryRecord>();
        public List<DayHistoryRecord> AllDaysHistory => allDaysHistory;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // This manager shares the scene object with managers that reference the Packing UI.
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            if (currentDayData != null && currentDayData.DailyJobs != null)
                templateJobs = new List<JobData>(currentDayData.DailyJobs);

            var saves = SaveLoadManager.Instance;
            if (saves != null)
            {
                if (saves.LoadGame()) return;
                UpgradeManager.Instance?.ResetAllUpgrades();
            }

            if (currentDayData != null) StartNewDay(currentDayData);
            else if (scheduledDays != null && scheduledDays.Count > 0 && scheduledDays[0] != null) StartNewDay(scheduledDays[0]);
            else StartNewDay(CreateDay(1));
        }

        private void Update()
        {
            // Dùng chuẩn New Input System (không dùng UnityEngine.Input nữa)
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame)
            {
                Debug.Log("<color=yellow>[CHEAT F3] Tua hết ngày ngay lập tức!</color>");
                EndDay(isManual: true);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        internal void RestoreProgress(int dayIndex, int gold)
        {
            PlayerGold = gold;
            bonusTimeBudgetFromUpgrade = UpgradeManager.Instance.GetBonusDayTimeBudget();
            dayIndex = Mathf.Clamp(dayIndex, 1, 7);
            DayData targetDay = (scheduledDays != null && dayIndex <= scheduledDays.Count && dayIndex > 0)
                ? scheduledDays[dayIndex - 1]
                : (currentDayData != null && currentDayData.DayIndex == dayIndex ? currentDayData : CreateDay(dayIndex));
            StartNewDay(targetDay);
        }

        private DayData CreateDay(int index)
        {
            DayData day = ScriptableObject.CreateInstance<DayData>();
            day.DayIndex = index;
            day.DayTimeBudget = 180f;
            day.DailyJobs = new List<JobData>(templateJobs);
            return day;
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
            if (dayData == null) dayData = CreateDay(currentDayIndex > 0 ? currentDayIndex : 1);
            
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

            currentActiveJob = null;
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
            bool hasTime = dayTimeRemaining >= job.TimeMin;
            bool hasStars = totalReputationStars >= job.RequiredStars; 
            return hasTime && hasStars; 
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
            totalReputationStars += stars;
            
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
            if (!isDayActive) return;
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

            allDaysHistory.Add(record);
            
            // Phát tin sự kiện kết thúc ngày
            EventBus.Publish(new GameEvent.DayEndedEvent(record));
            SaveLoadManager.Instance?.SaveGame();
        }
        
        private void HandleJobSubmitted(GameEvent.JobSubmittedEvent e)
        {
            DeductTimeAndRecordJob(e.JobResult.TotalJobTime, e.JobResult.EarnedGold, e.JobResult.Stars);
        }
        
        private void HandleNextDayRequested(GameEvent.NextDayRequestedEvent obj)
        {
            // Tăng chỉ số ngày và bắt đầu ngày mới với danh sách đơn hàng mẫu (tối đa Ngày 7)
            if (!CanSaveProgress || currentDayIndex >= 7) return;
            int nextDayIndex = currentDayIndex + 1;
            DayData nextDay = (scheduledDays != null && nextDayIndex <= scheduledDays.Count && nextDayIndex > 0)
                ? scheduledDays[nextDayIndex - 1]
                : CreateDay(nextDayIndex);
            
            // Nếu slot bị null thì tự động fallback tạo ngày mẫu, tránh văng crash
            if (nextDay == null)
            {
                nextDay = CreateDay(nextDayIndex);
            }
            
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
                var upgrades = UpgradeManager.Instance;
                var definition = upgrades?.GetUpgradeData(e.UpgradeID);
                if (definition != null)
                {
                    float addedTime = definition.GetEffectValue(e.NewLevel) -
                                      definition.GetEffectValue(e.NewLevel - 1);
                    IncreaseDayBudget(addedTime);
                }
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
        
        // ========== CÔNG CỤ TEST NHANH NGÀY 3, 5, 7 ==========

        [ContextMenu("TEST: Nhảy tới Ngày 3 (600$, 10 Sao)")]
        public void TestJumpToDay3()
        {
            playerGold = 600;
            totalReputationStars = 10;
            if (scheduledDays != null && scheduledDays.Count >= 3)
            {
                StartNewDay(scheduledDays[2]); // Ngày 3
            }
            Debug.Log("<color=cyan><b>[TEST] ĐÃ NHẢY TỚI NGÀY 3: Có 600$, 10⭐. Đơn VIP đã mở!</b></color>");
        }

        [ContextMenu("TEST: Nhảy tới Ngày 5 (1500$, 20 Sao)")]
        public void TestJumpToDay5()
        {
            playerGold = 1500;
            totalReputationStars = 20;
            if (scheduledDays != null && scheduledDays.Count >= 5)
            {
                StartNewDay(scheduledDays[4]); // Ngày 5
            }
            Debug.Log("<color=cyan><b>[TEST] ĐÃ NHẢY TỚI NGÀY 5: Có 1500$, 20⭐. Xe mở rộng!</b></color>");
        }

        [ContextMenu("TEST: Nhảy tới Ngày 7 (3500$, 28 Sao)")]
        public void TestJumpToDay7()
        {
            playerGold = 3500;
            totalReputationStars = 28;
            if (scheduledDays != null && scheduledDays.Count >= 7)
            {
                StartNewDay(scheduledDays[6]); // Ngày 7
            }
            Debug.Log("<color=cyan><b>[TEST] ĐÃ NHẢY TỚI NGÀY 7: Có 3500$, 28⭐. Sẵn sàng Siêu VIP trả nợ 5000$!</b></color>");
        }
    }
}
