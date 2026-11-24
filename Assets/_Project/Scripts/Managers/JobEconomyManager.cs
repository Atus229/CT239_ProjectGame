using UnityEngine;
using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Data;


public class JobEconomyManager : MonoBehaviour
{
    public static JobEconomyManager Instance { get; private set; }

    [Header("Rating Parameters")]
    [SerializeField] JobData currentJobData;
    [SerializeField] private int maxStars = 5;
    [SerializeField] private int minStars = 1;
    [SerializeField] private float timePenaltyFactorK = 5f; // K = 5 theo quy tắc BR-04

    [Header("Job Target Window")] 
    [SerializeField]
    private float timeMin = 40f;
    [SerializeField]
    private float timeMax = 60f;

    public float TimeMin => timeMin;
    public float TimeMax => timeMax;
    public JobData CurrentJobData => currentJobData;
    
    //Bien luu tong thoi gian va tong tien hien tai tren xe
    public float CurrentJobTime { get; private set; } = 0f;
    public int CurrentJobValue { get; private set; } = 0;
    private readonly List<ItemData> placedItems = new List<ItemData>();

    /// <summary>
    /// Tổng giá trị tối đa có thể kiếm được của tất cả các món đồ trong đơn hàng hiện tại
    /// </summary>
    public int TotalJobValue
    {
        get
        {
            if (currentJobData == null || currentJobData.ItemQueue == null) return 0;
            int sum = 0;
            foreach (var item in currentJobData.ItemQueue)
            {
                if (item != null) sum += item.Value;
            }
            return sum;
        }
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //Hàm nghe sự kiện được kích hoạt
    private void OnEnable()
    {
        EventBus.Subscribe<GameEvent.ItemPlacedEvent>(HandleItemPlaced);
        EventBus.Subscribe<GameEvent.ItemRemovedEvent>(HandleItemRemoved);
        EventBus.Subscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
        EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(HandleContinueFromJobResult);
        EventBus.Subscribe<GameEvent.DayEndedEvent>(HandleDayEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameEvent.ItemPlacedEvent>(HandleItemPlaced);
        EventBus.Unsubscribe<GameEvent.ItemRemovedEvent>(HandleItemRemoved);
        EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
        EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(HandleContinueFromJobResult);
        EventBus.Unsubscribe<GameEvent.DayEndedEvent>(HandleDayEnded);
    }

    private void Start()
    {
        if (currentJobData != null)
        {
            LoadDataJob(currentJobData);
        }
        else
        {
            NotifyStateChanged();//dung moc mac dinh
        }
    }

    /// <summary>
    /// BR-04: Tính số sao dựa trên thời gian bốc xếp thực tế (totalJobTime), Tmin và Tmax.
    /// </summary>
    public int CalculateStarRating(float totalJobTime, float tMin, float tMax)
    {
        // Giao hàng đúng hẹn (Tmin <= TotalJobTime <= Tmax)
        if (totalJobTime <= tMax)
        {
            return maxStars;
        }

        // Giao trễ (TotalJobTime > Tmax) -> Phạt 1 sao cho mỗi K giây quá hạn
        float overTime = totalJobTime - tMax;
        int penalty = Mathf.CeilToInt(overTime / timePenaltyFactorK);
        int calculatedStars = maxStars - penalty;

        return Mathf.Max(minStars, calculatedStars);
    }
    

    /// <summary>
    /// BR-02: Kiểm tra đơn hàng có đủ điều kiện nhận trên Bảng việc (DayTimeRemaining >= Tmin).
    /// </summary>
    public bool CanAcceptJob(float dayTimeRemaining, float tMin)
    {
        return dayTimeRemaining >= tMin;
    }

    /// <summary>
    /// BR-03: Kiểm tra điều kiện bật nút "Xác nhận vận chuyển" (TotalJobTime >= Tmin, Value >= RequiredMinGold, có món đồ bắt buộc).
    /// </summary>
    public bool CanSubmitJob(float totalJobTime, float tMin)
    {
        int minRequiredGold = (currentJobData != null) ? currentJobData.RequiredMinGold : 0;
        return totalJobTime >= tMin && CurrentJobValue >= minRequiredGold && HasRequiredItem();
    }

    /// <summary>
    /// Kiểm tra xem đơn hàng hiện tại có yêu cầu món đồ bắt buộc không, và người chơi đã xếp món đó lên xe chưa.
    /// </summary>
    public bool HasRequiredItem()
    {
        if (currentJobData == null || currentJobData.RequiredItem == null)
        {
            return true; // Đơn không yêu cầu món đồ bắt buộc -> Thỏa mãn
        }

        ItemData required = currentJobData.RequiredItem;

        // 1. Kiểm tra trực tiếp trên lưới thùng xe (GridManager)
        if (GridManager.Instance != null && GridManager.Instance.HasItemOnGrid(required))
        {
            return true;
        }

        // 2. Kiểm tra phụ trợ qua danh sách món đồ đã bắt được qua sự kiện
        for (int i = 0; i < placedItems.Count; i++)
        {
            ItemData item = placedItems[i];
            if (item != null && (item == required || (!string.IsNullOrEmpty(item.ItemID) && item.ItemID == required.ItemID)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Hàm tổng hợp tính toán kết quả khi hoàn thành đơn hàng.
    /// </summary>
    public JobResult CalculateJobResult(float totalJobTime, float tMin, float tMax, int totalItemValue)
    {
        int stars = CalculateStarRating(totalJobTime, tMin, tMax);

        return new JobResult
        {
            Stars = stars,
            EarnedGold = totalItemValue,
            TotalJobTime = totalJobTime,
            IsOnTime = totalJobTime <= tMax
        };
    }

    /// <summary>
    /// Gọi khi người chơi bấm nút "Xác nhận vận chuyển".
    /// Tự động lấy dữ liệu sẵn có trong máy để tính sao và tiền thưởng.
    /// </summary>
    public JobResult CalculateJobResult()
    {
        int star = CalculateStarRating(CurrentJobTime, timeMin, timeMax);

        return new JobResult()
        {
            Stars = star,
            EarnedGold = CurrentJobValue,
            TotalJobTime = CurrentJobTime,
            IsOnTime = CurrentJobTime <= timeMax
        };
    }

    /// <summary>
    /// Hàm này được gọi mỗi khi nguoi choi chon, bắt đầu một đơn hàng mới (hoặc chơi lại màn đó)
    /// </summary>
    public void LoadDataJob(JobData job)
    {
        if (job == null) return;

        //cap nhat moc min max tu don hanng moi
        timeMin = job.TimeMin;
        timeMax = job.TimeMax;
        
        //Reset thoi gian, tien va danh sach mon do da xep
        CurrentJobTime = 0f;
        CurrentJobValue = 0;
        placedItems.Clear();

        currentJobData = job;
        
        //bao cho UI biet -> don hang moi da vao
        NotifyStateChanged();
    }

    /// <summary>
    /// Đóng gói dữ liệu hiện tại và phát sóng cho UI
    /// </summary>
    private void NotifyStateChanged()
    {
        //Phat tin cho TimeGaugeUI
        EventBus.Publish(new GameEvent.JobTimeUpdatedEvent(CurrentJobTime, timeMin, timeMax));
        
        //Phat tin cho nut xac nhan don hang 
        //dieu kien: CurrentJobTime >= timeMin, CurrentJobValue >= RequiredMinGold, va da dat mon do bat buoc
        int minRequiredGold = (currentJobData != null) ? currentJobData.RequiredMinGold : 0;
        bool hasRequiredItem = HasRequiredItem();
        bool canConfirm = CurrentJobTime >= timeMin && CurrentJobValue >= minRequiredGold && hasRequiredItem;

        EventBus.Publish(new GameEvent.JobConfirmStateChangedEvent(canConfirm));
    }
    
    /// <summary>
    /// Tự động chạy khi người chơi nhấc/gỡ 1 món đồ ra khỏi ô lưới
    /// </summary>
    private void HandleItemRemoved(GameEvent.ItemRemovedEvent e)
    {
        if (e.Item == null) return;
        
        // Xoa 1 mon phu hop khoi danh sach placedItems
        for (int i = placedItems.Count - 1; i >= 0; i--)
        {
            ItemData it = placedItems[i];
            if (it == e.Item || (it != null && !string.IsNullOrEmpty(it.ItemID) && it.ItemID == e.Item.ItemID))
            {
                placedItems.RemoveAt(i);
                break;
            }
        }

        //Tru bot tien va thoi gian khi go item ra
        //Su dung Mathf.Max -> khong de gia tri bi am
        CurrentJobTime = Mathf.Max(0f, CurrentJobTime - e.Item.HandlingTime);
        CurrentJobValue = Mathf.Max(0, CurrentJobValue - e.Item.Value);
        
        //Tinh toan lai va bao tin len UI
        NotifyStateChanged();
    }

    /// <summary>
    /// Tự động chạy khi GridManager báo có 1 món đồ vừa được đặt thành công vào ô lưới
    /// </summary>
    private void HandleItemPlaced(GameEvent.ItemPlacedEvent e)
    {
        if (e.Item == null) return;
        
        placedItems.Add(e.Item);

        //Cong thoi gian va gia tri cua mon hang
        CurrentJobTime += e.Item.HandlingTime;
        CurrentJobValue += e.Item.Value;
        
        //Tinh toan xong thi bao tin cho UI
        NotifyStateChanged();
    }
    
    private void HandleJobSelected(GameEvent.JobSelectedEvent e)
    {
        if (e.SelectedJob != null)
        {
            LoadDataJob(e.SelectedJob);
        }
    }

    private void HandleContinueFromJobResult(GameEvent.ContinueFromJobResultEvent e)
    {
        placedItems.Clear();
    }

    private void HandleDayEnded(GameEvent.DayEndedEvent e)
    {
        placedItems.Clear();
    }
}

[Serializable]
public struct JobResult
{
    public int Stars;
    public int EarnedGold;
    public float TotalJobTime;
    public bool IsOnTime;
}

