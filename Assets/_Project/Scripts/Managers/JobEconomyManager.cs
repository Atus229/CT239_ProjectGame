using UnityEngine;

public class JobEconomyManager : MonoBehaviour
{
    public static JobEconomyManager Instance { get; private set; }

    [Header("Rating Parameters")]
    [SerializeField] private int maxStars = 5;
    [SerializeField] private int minStars = 1;
    [SerializeField] private float timePenaltyFactorK = 5f; // K = 5 theo quy tắc BR-04

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        int penalty = Mathf.FloorToInt(overTime / timePenaltyFactorK);
        int calculatedStars = maxStars - penalty;

        return Mathf.Max(minStars, calculatedStars);
    }

    /// <summary>
    /// BR-05: Tính tiền thưởng thu được dựa trên tổng giá trị hàng hóa và hệ số thưởng.
    /// </summary>
    public int CalculateEarnedGold(int totalItemValue, float rewardMultiplier)
    {
        float gold = totalItemValue * rewardMultiplier;
        return Mathf.Max(0, Mathf.RoundToInt(gold));
    }

    /// <summary>
    /// BR-02: Kiểm tra đơn hàng có đủ điều kiện nhận trên Bảng việc (DayTimeRemaining >= Tmin).
    /// </summary>
    public bool CanAcceptJob(float dayTimeRemaining, float tMin)
    {
        return dayTimeRemaining >= tMin;
    }

    /// <summary>
    /// BR-03: Kiểm tra điều kiện bật nút "Xác nhận vận chuyển" (TotalJobTime >= Tmin).
    /// </summary>
    public bool CanSubmitJob(float totalJobTime, float tMin)
    {
        return totalJobTime >= tMin;
    }

    /// <summary>
    /// Hàm tổng hợp tính toán kết quả khi hoàn thành đơn hàng.
    /// </summary>
    public JobResult CalculateJobResult(float totalJobTime, float tMin, float tMax, int totalItemValue, float rewardMultiplier)
    {
        int stars = CalculateStarRating(totalJobTime, tMin, tMax);
        int gold = CalculateEarnedGold(totalItemValue, rewardMultiplier);

        return new JobResult
        {
            Stars = stars,
            EarnedGold = gold,
            TotalJobTime = totalJobTime,
            IsOnTime = totalJobTime <= tMax
        };
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