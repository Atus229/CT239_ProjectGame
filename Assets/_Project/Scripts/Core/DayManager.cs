using System;
using System.Collections.Generic;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Config & Runtime Data")]
    [SerializeField] private DayData currentDayData;
    private float dayTimeBudget;      // Quỹ thời gian tổng của ngày[cite: 2]
    private float dayTimeRemaining;   // Quỹ thời gian còn lại[cite: 2]
    private int currentDayIndex;

    [Header("Daily Statistics")]
    private int completedJobsCount;
    private int totalEarnedInDay;
    private int totalStarsInDay;

    // Events thông báo cho UI và các Manager khác
    public event Action<float, float> OnTimeRemainingChanged; // (Remaining, Budget)
    public event Action OnDayStarted;
    public event Action OnDayEnded;

    public float TimeRemaining => dayTimeRemaining; //[cite: 2]
    public float TimeBudget => dayTimeBudget;       //[cite: 2]
    public int CurrentDayIndex => currentDayIndex;

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

    /// <summary>
    /// Bắt đầu một ngày làm việc mới[cite: 2]
    /// </summary>
    public void StartNewDay(DayData dayData)
    {
        currentDayData = dayData;
        currentDayIndex = dayData.DayIndex;
        dayTimeBudget = dayData.DayTimeBudget;     //[cite: 2]
        dayTimeRemaining = dayTimeBudget;          //[cite: 2]

        completedJobsCount = 0;
        totalEarnedInDay = 0;
        totalStarsInDay = 0;

        OnTimeRemainingChanged?.Invoke(dayTimeRemaining, dayTimeBudget);
        OnDayStarted?.Invoke();
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
    /// Trừ quỹ thời gian sau khi hoàn tất một đơn[cite: 2]
    /// </summary>
    public void DeductTimeAndRecordJob(float totalJobTime, int earnedMoney, int stars)
    {
        dayTimeRemaining -= totalJobTime; // Trừ đúng số giây thực tế[cite: 2]
        
        completedJobsCount++;
        totalEarnedInDay += earnedMoney;
        totalStarsInDay += stars;

        OnTimeRemainingChanged?.Invoke(dayTimeRemaining, dayTimeBudget);
    }

    /// <summary>
    /// Kiểm tra điều kiện tự động kết thúc ngày khi không còn đơn nào đủ điều kiện[cite: 2]
    /// </summary>
    public void CheckAutoEndDayCondition(List<JobData> activeJobsOnBoard)
    {
        bool hasValidJob = false;
        if (activeJobsOnBoard != null)
        {
            foreach (var job in activeJobsOnBoard)
            {
                if (CanAcceptJob(job)) //[cite: 2]
                {
                    hasValidJob = true;
                    break;
                }
            }
        }

        // Nếu không còn đơn nào đủ điều kiện nhận -> Tự động kết thúc ngày[cite: 2]
        if (!hasValidJob)
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

        // Gọi SaveLoadManager lưu dữ liệu vào cuối ngày[cite: 2]
        SaveLoadManager.Instance?.SaveDailyProgress(record);

        // Hiển thị Màn hình Day Summary[cite: 2]
        UIManager.Instance?.ShowDaySummaryUI(record);
    }
}