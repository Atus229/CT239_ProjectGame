using UnityEngine;

namespace _Project.Scripts.Tester
{
    public class JobEconomyTestRunner : MonoBehaviour
    {
        [Header("Cấu hình kiểm thử")] [SerializeField]
        private bool runOnStart = true;

        [SerializeField] private float timePenaltyFactorK = 5f; // Hằng số K = 5 theo BR-04 trong GDD

        private void Start()
        {
            if (runOnStart)
            {
                RunAllTests();
            }
        }

        [ContextMenu("Chạy Kiểm Thử Ngay (Run Tests)")]
        public void RunAllTests()
        {
            Debug.Log("<color=#00FF00><b>========== BẮT ĐẦU KIỂM THỬ JOB ECONOMY (TUẦN 2) ==========</b></color>");

            TestStarRating();
            TestGoldReward();

            Debug.Log("<color=#00FF00><b>========== KẾT THÚC KIỂM THỬ: TẤT CẢ ĐÃ HOÀN TẤT ==========</b></color>");
        }

        private void TestStarRating()
        {
            Debug.Log("<b>--- KIỂM THỬ RATING SAO (BR-04: Tmin=20s, Tmax=60s, K=5) ---</b>");

            // 1. Trong hạn (<= 60s): 5 sao
            int star1 = CalculateStarRating(55f, 20f, 60f);
            LogTestResult("55s (Trong hạn 20-60s)", star1, 5, "sao");

            // 2. Trễ 4s (< K=5s): 5 sao (Floor(4/5) = 0)
            int star2 = CalculateStarRating(64f, 20f, 60f);
            LogTestResult("64s (Trễ 4s < K=5s)", star2, 5, "sao");

            // 3. Trễ 5s (= K=5s): 4 sao (Floor(5/5) = 1)
            int star3 = CalculateStarRating(65f, 20f, 60f);
            LogTestResult("65s (Trễ 5s -> trừ 1 sao)", star3, 4, "sao");

            // 4. Trễ 25s: mức sàn 1 sao (Floor(25/5) = 5 -> 5-5=0 -> lấy min=1)
            int star4 = CalculateStarRating(85f, 20f, 60f);
            LogTestResult("85s (Trễ 25s -> chạm sàn)", star4, 1, "sao");
        }

        private void TestGoldReward()
        {
            Debug.Log("<b>--- KIỂM THỬ THƯỞNG VÀNG (BR-05) ---</b>");

            // Phiên bản MVP: Tiền thưởng = Tổng giá trị các món hàng đã đặt
            int gold1 = CalculateEarnedGold(120);
            LogTestResult("Tổng giá trị hàng 120", gold1, 120, "vàng");
        }

        private int CalculateStarRating(float totalJobTime, float tMin, float tMax)
        {
            if (totalJobTime <= tMax)
                return 5;

            float overTime = totalJobTime - tMax;
            int penalty = Mathf.FloorToInt(overTime / timePenaltyFactorK);
            int calculatedStars = 5 - penalty;

            return Mathf.Max(1, calculatedStars);
        }

        private int CalculateEarnedGold(int totalItemValue)
        {
            // GDD Mục 4.3: Bản MVP thưởng tiền thuần bằng tổng giá trị hàng hóa
            return Mathf.Max(0, totalItemValue);
        }

        private void LogTestResult(string testCaseName, int actual, int expected, string unit)
        {
            if (actual == expected)
            {
                Debug.Log(
                    $"<color=#4CAF50>✔ [PASS]</color> {testCaseName} => Thực tế: <b>{actual} {unit}</b> (Kỳ vọng: {expected})");
            }
            else
            {
                Debug.LogError(
                    $"<color=#F44336>✘ [FAIL]</color> {testCaseName} => Thực tế: <b>{actual} {unit}</b> nhưng Kỳ vọng: {expected}");
            }
        }
    }
}