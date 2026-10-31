using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using _Project.Scripts.Managers;

namespace _Project.Scripts.Tester
{
    /// <summary>
    /// Kịch bản Unit Test tự động cho hệ thống UpgradeManager và UpgradeData.
    /// Hỗ trợ chạy tự động lúc khởi chạy (Play Mode) hoặc gọi trực tiếp từ ContextMenu.
    /// </summary>
    public class UpgradeManagerTestRunner : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;

        private int totalTests = 0;
        private int passedTests = 0;

        private void Start()
        {
            if (runOnStart)
            {
                RunAllTests();
            }
        }

        [ContextMenu("Chạy Kiểm Thử UpgradeManager")]
        public void RunAllTests()
        {
            totalTests = 0;
            passedTests = 0;

            Debug.Log("<color=#00E5FF><b>=====================================================</b></color>");
            Debug.Log("<color=#00E5FF><b>=== BẮT ĐẦU KIỂM THỬ HỆ THỐNG NÂNG CẤP (UPGRADE SYSTEM) ===</b></color>");
            Debug.Log("<color=#00E5FF><b>=====================================================</b></color>");

            GameObject testObject = new GameObject("[TestEnv] UpgradeManager");
            UpgradeManager manager = testObject.AddComponent<UpgradeManager>();

            var testCatalog = CreateMockCatalog();
            manager.SetAvailableUpgrades(testCatalog);

            Test_1_InitialState(manager);
            Test_2_CostCalculationFormula(testCatalog[0]);
            Test_3_SuccessfulPurchase(manager);
            Test_4_InsufficientFunds(manager);
            Test_5_MaxLevelBoundary(manager);
            Test_6_GameplayEffectsCalculation(manager);
            Test_7_SaveLoadIntegrity(manager);

            Destroy(testObject);

            Debug.Log("<color=#00E5FF><b>-----------------------------------------------------</b></color>");
            if (passedTests == totalTests)
            {
                Debug.Log($"<color=#4CAF50><b>✔ HOÀN TẤT TẤT CẢ KIỂM THỬ: {passedTests}/{totalTests} TEST CASES ĐÃ ĐẠT!</b></color>");
            }
            else
            {
                Debug.LogError($"<color=#F44336><b>✘ CÓ LỖI: CHỈ ĐẠT {passedTests}/{totalTests} TEST CASES!</b></color>");
            }
            Debug.Log("<color=#00E5FF><b>=====================================================</b></color>");
        }

        /// <summary>
        /// Tạo danh mục 4 gói nâng cấp chuẩn cho môi trường Test.
        /// </summary>
        private List<UpgradeData> CreateMockCatalog()
        {
            var list = new List<UpgradeData>();

            // 1. DayTimeBudget
            var upg1 = ScriptableObject.CreateInstance<UpgradeData>();
            upg1.Init(1, "Mở rộng Quỹ thời gian ngày", UpgradeData.TypeDayTimeBudget, 100, 2.5f, 3, 30f);
            list.Add(upg1);

            // 2. VehicleCapacity
            var upg2 = ScriptableObject.CreateInstance<UpgradeData>();
            upg2.Init(2, "Mở rộng sức chứa xe", UpgradeData.TypeVehicleCapacity, 150, 2.3333333f, 2, 1f);
            list.Add(upg2);

            // 3. HandlingEfficiency
            var upg3 = ScriptableObject.CreateInstance<UpgradeData>();
            upg3.Init(3, "Nâng cấp hiệu suất xử lý", UpgradeData.TypeHandlingEfficiency, 200, 2.25f, 2, 0.10f);
            list.Add(upg3);

            // 4. LateGraceBuffer
            var upg4 = ScriptableObject.CreateInstance<UpgradeData>();
            upg4.Init(4, "Bảo hiểm trễ hẹn & Uy tín", UpgradeData.TypeLateGraceBuffer, 300, 2.0f, 2, 5f);
            list.Add(upg4);

            return list;
        }

        /// <summary>
        /// Test 1: Người chơi mới phải có cấp độ 0 cho tất cả nâng cấp.
        /// </summary>
        private void Test_1_InitialState(UpgradeManager mgr)
        {
            bool pass = mgr.GetUpgradeLevel(1) == 0 &&
                        mgr.GetUpgradeLevel(2) == 0 &&
                        mgr.GetUpgradeLevel(3) == 0 &&
                        mgr.GetUpgradeLevel(4) == 0;

            AssertCondition("Test 1: Khởi tạo ban đầu (Tất cả cấp độ đều bằng 0)", pass);
        }

        /// <summary>
        /// Test 2: Công thức tính chi phí BaseCost * (costGrowth ^ (level - 1)).
        /// </summary>
        private void Test_2_CostCalculationFormula(UpgradeData data)
        {
            // BaseCost = 100, Growth = 2.5, MaxLevel = 3
            int costL1 = data.GetCostForLevel(1); // 100
            int costL2 = data.GetCostForLevel(2); // 100 * 2.5 = 250
            int costL3 = data.GetCostForLevel(3); // 100 * 6.25 = 625
            int costL4 = data.GetCostForLevel(4); // Vượt max level -> -1

            bool pass = (costL1 == 100) && (costL2 == 250) && (costL3 == 625) && (costL4 == -1);
            AssertCondition($"Test 2: Công thức tăng trưởng chi phí (C1: {costL1}, C2: {costL2}, C3: {costL3}, C4: {costL4})", pass);
        }

        /// <summary>
        /// Test 3: Giao dịch thành công (Có đủ tiền, tăng cấp và phát sự kiện).
        /// </summary>
        private void Test_3_SuccessfulPurchase(UpgradeManager mgr)
        {
            int playerMoney = 500;
            bool eventTriggered = false;
            int receivedNewLevel = -1;

            void OnPurchased(GameEvent.UpgradePurchasedEvent e)
            {
                if (e.UpgradeID == 1)
                {
                    eventTriggered = true;
                    receivedNewLevel = e.NewLevel;
                }
            }

            EventBus.Subscribe<GameEvent.UpgradePurchasedEvent>(OnPurchased);

            // Mua Cấp 1 (chi phí 100)
            bool purchaseSuccess = mgr.TryPurchaseUpgrade(1, ref playerMoney);

            EventBus.Unsubscribe<GameEvent.UpgradePurchasedEvent>(OnPurchased);

            bool pass = purchaseSuccess &&
                        (playerMoney == 400) &&
                        (mgr.GetUpgradeLevel(1) == 1) &&
                        eventTriggered &&
                        (receivedNewLevel == 1);

            AssertCondition("Test 3: Mua thành công cấp 1 (Tiền còn 400, Cấp lên 1, Sự kiện kích hoạt)", pass);
        }

        /// <summary>
        /// Test 4: Không đủ tiền (Luồng thay thế [A1] trong SRS).
        /// </summary>
        private void Test_4_InsufficientFunds(UpgradeManager mgr)
        {
            // Cấp hiện tại là 1, muốn nâng cấp 2 cần 250 tiền.
            int playerMoney = 150; // Không đủ 250

            bool canUpgrade = mgr.CanUpgrade(1, playerMoney);
            bool purchaseSuccess = mgr.TryPurchaseUpgrade(1, ref playerMoney);

            bool pass = (!canUpgrade) &&
                        (!purchaseSuccess) &&
                        (playerMoney == 150) && // Tiền không bị trừ
                        (mgr.GetUpgradeLevel(1) == 1); // Cấp không đổi

            AssertCondition("Test 4: Luồng thay thế [A1] Thiếu tiền (Bị từ chối, Tiền giữ nguyên, Cấp không đổi)", pass);
        }

        /// <summary>
        /// Test 5: Đạt cấp tối đa (Luồng thay thế [A2] trong SRS).
        /// </summary>
        private void Test_5_MaxLevelBoundary(UpgradeManager mgr)
        {
            int playerMoney = 2000;

            // Nâng lên Cấp 2 (chi phí 250)
            mgr.TryPurchaseUpgrade(1, ref playerMoney);
            // Nâng lên Cấp 3 - MaxLevel (chi phí 625)
            mgr.TryPurchaseUpgrade(1, ref playerMoney);

            int currentLevel = mgr.GetUpgradeLevel(1); // Phải bằng 3
            int nextCost = mgr.GetNextUpgradeCost(1); // Phải bằng -1

            // Thử mua tiếp khi đã max level
            bool canUpgradeFurther = mgr.CanUpgrade(1, playerMoney);
            bool purchaseExceeded = mgr.TryPurchaseUpgrade(1, ref playerMoney);

            bool pass = (currentLevel == 3) &&
                        (nextCost == -1) &&
                        (!canUpgradeFurther) &&
                        (!purchaseExceeded);

            AssertCondition("Test 5: Luồng thay thế [A2] Max Level (Cấp 3/3, NextCost=-1, Chặn mua thêm)", pass);
        }

        /// <summary>
        /// Test 6: Kiểm tra tính toán các chỉ số hiệu ứng Gameplay.
        /// </summary>
        private void Test_6_GameplayEffectsCalculation(UpgradeManager mgr)
        {
            // UpgradeID 1 (DayTimeBudget) hiện đang ở Cấp 3 -> Hiệu ứng = 30s * 3 = 90s
            float bonusBudget = mgr.GetBonusDayTimeBudget();

            // Mua UpgradeID 3 (HandlingEfficiency) lên Cấp 1 -> giảm 10% (0.10) -> multiplier = 0.90f
            int money = 3000; // Cần đủ tiền để mua: U3 (200) + U2 (150) + U4 C1 (300) + U4 C2 (600) = 1250đ
            mgr.TryPurchaseUpgrade(3, ref money);
            float handlingMultiplier = mgr.GetHandlingTimeMultiplier();

            // Mua UpgradeID 2 (VehicleCapacity) lên Cấp 1 -> +1 cột
            mgr.TryPurchaseUpgrade(2, ref money);
            int bonusCols = mgr.GetBonusGridColumns();

            // Mua UpgradeID 4 (LateGraceBuffer) lên Cấp 2 -> +5s * 2 = 10s (Chi phí C1: 300, C2: 600)
            mgr.TryPurchaseUpgrade(4, ref money);
            mgr.TryPurchaseUpgrade(4, ref money);
            float lateBuffer = mgr.GetLateGraceBuffer();

            bool pass = Mathf.Approximately(bonusBudget, 90f) &&
                        Mathf.Approximately(handlingMultiplier, 0.90f) &&
                        (bonusCols == 1) &&
                        Mathf.Approximately(lateBuffer, 10f);

            AssertCondition($"Test 6: Tính toán chỉ số hiệu ứng (Budget: +{bonusBudget}s, Multiplier: {handlingMultiplier}, Cols: +{bonusCols}, Buffer: +{lateBuffer}s)", pass);
        }

        /// <summary>
        /// Test 7: Kiểm tra tính vẹn toàn khi Save / Load Player_Upgrade.
        /// </summary>
        private void Test_7_SaveLoadIntegrity(UpgradeManager mgr)
        {
            // Xuất dữ liệu hiện tại (U1: Lvl 3, U2: Lvl 1, U3: Lvl 1, U4: Lvl 2)
            var savedData = mgr.ExportPlayerUpgrades();

            // Giả lập Reset toàn bộ cấp độ về 0
            mgr.ResetAllUpgrades();
            bool isReset = (mgr.GetUpgradeLevel(1) == 0 && mgr.GetUpgradeLevel(4) == 0);

            // Nạp lại dữ liệu đã lưu
            mgr.ImportPlayerUpgrades(savedData);

            bool isRestored = (mgr.GetUpgradeLevel(1) == 3) &&
                              (mgr.GetUpgradeLevel(2) == 1) &&
                              (mgr.GetUpgradeLevel(3) == 1) &&
                              (mgr.GetUpgradeLevel(4) == 2);

            bool pass = isReset && isRestored && (savedData.Count == 4);
            AssertCondition("Test 7: Tính toàn vẹn Save/Load (Export -> Reset -> Import -> Khôi phục chính xác 100%)", pass);
        }

        private void AssertCondition(string testName, bool condition)
        {
            totalTests++;
            if (condition)
            {
                passedTests++;
                Debug.Log($"<color=#4CAF50>✔ [PASS]</color> <b>{testName}</b>");
            }
            else
            {
                Debug.LogError($"<color=#F44336>✘ [FAIL]</color> <b>{testName}</b>");
            }
        }
    }
}
