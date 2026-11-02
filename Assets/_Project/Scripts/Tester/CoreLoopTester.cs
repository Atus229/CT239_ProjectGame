using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using UnityEngine.InputSystem;

public class CoreLoopTester : MonoBehaviour
{
    [Header("Test Assets")]
    [SerializeField] private DayData testDayData;
    [SerializeField] private JobData testJobSmall;
    [SerializeField] private JobData testJobMedium;
    [SerializeField] private JobData testJobLarge;

    private void Start()
    {
        // Lắng nghe các event từ DayManager phát ra để in log kiểm tra
        EventBus.Subscribe<GameEvent.DayStartedEvent>(OnDayStarted);
        EventBus.Subscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
        EventBus.Subscribe<GameEvent.DayEndedEvent>(OnDayEnded);

        // Khởi động ngày 1
        if (DayManager.Instance != null && testDayData != null)
        {
            DayManager.Instance.StartNewDay(testDayData);
        }
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameEvent.DayStartedEvent>(OnDayStarted);
        EventBus.Unsubscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
        EventBus.Unsubscribe<GameEvent.DayEndedEvent>(OnDayEnded);
    }

    // Các hàm lắng nghe sự kiện để kiểm tra
    private void OnDayStarted(GameEvent.DayStartedEvent e)
    {
        Debug.Log($"<color=green><b>[DAY STARTED]</b></color> Ngày {e.dayIndex} bắt đầu với quỹ thời gian: {e.dayTimeBudget}s");
    }

    private void OnDayTimeUpdated(GameEvent.DayTimeUpdatedEvent e)
    {
        Debug.Log($"<color=yellow><b>[TIME UPDATED]</b></color> Quỹ thời gian còn lại: {e.RemainingTime}s / {e.Budget}s");
    }

    private void OnDayEnded(GameEvent.DayEndedEvent e)
    {
        Debug.Log($"<color=red><b>[DAY ENDED]</b></color> Kết thúc ngày! Đã xong: {e.record.jobsCompleted} đơn, Kiếm được: {e.record.totalEarned} vàng, Thời gian sử dụng: {e.record.usedTime}s");
    }

    // Giả lập các thao tác người chơi bằng phím bấm
    private void Update()
    {
        // Đảm bảo bàn phím tồn tại
        if (Keyboard.current == null) return;

        // 1. Phím 1 (Số 1 trên bàn phím chính)
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log(">> [TEST] Giả lập: Người chơi CHỌN ĐƠN NHỎ (Job_Small)");
            EventBus.Publish(new GameEvent.JobSelectedEvent(testJobSmall));
        }

        // 2. Phím 2 (Số 2 trên bàn phím chính)
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log(">> [TEST] Giả lập: Hoàn tất đơn 40s");
            JobResult result = new JobResult
            {
                TotalJobTime = 40f,
                Stars = 5,
                EarnedGold = 100,
                IsOnTime = true
            };
            EventBus.Publish(new GameEvent.JobSubmittedEvent(testJobSmall, result));
        }

        // 3. Phím Space
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log(">> [TEST] Giả lập: Bấm TIẾP TỤC từ màn hình kết quả");
            EventBus.Publish(new GameEvent.ContinueFromJobResultEvent());
        }

        // 4. Phím N
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            Debug.Log(">> [TEST] Giả lập: Bắt đầu NGÀY MỚI");
            EventBus.Publish(new GameEvent.NextDayRequestedEvent());
        }
    }
}