using System;
using System.Collections.Generic;
using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        [Header("Day Config & Runtime Data")] [SerializeField]
        private DayData currentDayData;

        private float dayTimeBudget; // Quỹ thời gian tổng của ngày[cite: 2]
        private float dayTimeRemaining; // Quỹ thời gian còn lại[cite: 2]
        private int currentDayIndex;

        [Header("Daily Statistics")] private int completedJobsCount;
        private int totalEarnedInDay;
        private int totalStarsInDay;

        // Events thông báo cho UI và các Manager khác
        public event Action<float, float> OnTimeRemainingChanged; // (Remaining, Budget)
        public event Action OnDayStarted;
        public event Action OnDayEnded;

        public float TimeRemaining => dayTimeRemaining; 
        public float TimeBudget => dayTimeBudget; 
        public int CurrentDayIndex => currentDayIndex;

        private JobData currentActiveJob;
        private JobResult lastJobResult;
        private List<JobData> activeJobsOnBoard = new List<JobData>();

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

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
            EventBus.Subscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Subscribe<GameEvent.JobCompletedEvent>(HandleJobCompleted);
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(HandleContinueFromJobResult);
            EventBus.Subscribe<GameEvent.NextDayRequestedEvent>(HandleNextDayRequested);
        }
        

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
            EventBus.Unsubscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Unsubscribe<GameEvent.JobCompletedEvent>(HandleJobCompleted);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(HandleContinueFromJobResult);
            EventBus.Unsubscribe<GameEvent.NextDayRequestedEvent>(HandleNextDayRequested);
        }
        
        /// <summary>
        /// Bắt đầu một ngày làm việc mới[cite: 2]
        /// </summary>
        public void StartNewDay(DayData dayData)
        {
            currentDayData = dayData;
            currentDayIndex = dayData.DayIndex;
            dayTimeBudget = dayData.DayTimeBudget; 
            dayTimeRemaining = dayTimeBudget;
            activeJobsOnBoard = new List<JobData>(dayData.DailyJobs);

            completedJobsCount = 0;
            totalEarnedInDay = 0;
            totalStarsInDay = 0;

            OnTimeRemainingChanged?.Invoke(dayTimeRemaining, dayTimeBudget);
            OnDayStarted?.Invoke();
            
            EventBus.Publish(new GameEvent.DayStartedEvent(currentDayIndex, dayTimeBudget));
            EventBus.Publish(new GameEvent.DayTimeUpdatedEvent(dayTimeRemaining, dayTimeBudget));
        }

        /// <summary>
        /// Kiểm tra xem một đơn hàng có thể nhận hay không[cite: 2]
        /// Điều kiện: Day Time Remaining >= Tmin của đơn[cite: 2]
        /// </summary>
        public bool CanAcceptJob(JobData job)
        {
            if (job == null) return false;
            return dayTimeRemaining >= job.TimeMin; //[cite: 2]
        }

        /// <summary>
        /// Trừ quỹ thời gian sau khi hoàn tất một đơn
        /// </summary>
        public void DeductTimeAndRecordJob(float totalJobTime, int earnedMoney, int stars)
        {
            dayTimeRemaining = Mathf.Max(0f, dayTimeRemaining - totalJobTime);

            completedJobsCount++;
            totalEarnedInDay += earnedMoney;
            totalStarsInDay += stars;

            OnTimeRemainingChanged?.Invoke(dayTimeRemaining, dayTimeBudget);
            
            EventBus.Publish(new GameEvent.DayTimeUpdatedEvent(dayTimeRemaining, dayTimeBudget));
        }

        /// <summary>
        /// Kiểm tra điều kiện tự động kết thúc ngày khi không còn đơn nào đủ điều kiện[cite: 2]
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

            // Nếu không còn đơn nào đủ điều kiện nhận -> Tự động kết thúc ngày[cite: 2]
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
        /// Kết thúc ngày làm việc[cite: 2]
        /// </summary>
        public void EndDay(bool isManual = false)
        {
            OnDayEnded?.Invoke();

            float avgStars = completedJobsCount > 0 ? (float)totalStarsInDay / completedJobsCount : 0f;

            DayHistoryRecord record = new DayHistoryRecord
            {
                dayIndex = currentDayIndex,
                jobsCompleted = completedJobsCount,
                totalEarned = totalEarnedInDay,
                averageStars = avgStars,
                usedTime = dayTimeBudget - Mathf.Max(0, dayTimeRemaining)
            };

            //Phát tin sự kiện kết thúc ngày
            EventBus.Publish(new GameEvent.DayEndedEvent(record));
        }
        
        private void HandleJobSubmitted(GameEvent.JobSubmittedEvent e)
        {
            DeductTimeAndRecordJob(e.JobResult.TotalJobTime,e.JobResult.EarnedGold, e.JobResult.Stars);
        }
        
        private void HandleNextDayRequested(GameEvent.NextDayRequestedEvent obj)
        {
            //Gia su co cau hinh du lieu ngay tiep theo hoac nap lai currenDayData
            if (currentDayData != null)
            {
                StartNewDay(currentDayData);
            }
        }

        
        private void HandleContinueFromJobResult(GameEvent.ContinueFromJobResultEvent obj)
        {
           //Tru thoi gian hoan thanh cong viec va ghi nhan thong ke
           DeductTimeAndRecordJob(lastJobResult.TotalJobTime, lastJobResult.EarnedGold, lastJobResult.Stars);
           
           //Xoa don ra khoi bang viec
           if (currentActiveJob != null)
           {
               activeJobsOnBoard.Remove(currentActiveJob);
               currentActiveJob = null;
           }
           
           //Kiem tra dieu kien ket thuc ngay
           CheckAutoEndDayCondition(activeJobsOnBoard);
        }

        //Nguoi choi xem bang ket qua(truoc khi nhan tiep tuc)
        private void HandleJobCompleted(GameEvent.JobCompletedEvent e)
        {
            lastJobResult = e.result;
        }

        private void HandleJobSelected(GameEvent.JobSelectedEvent e)
        {
            currentActiveJob = e.job;
        }
    }
}