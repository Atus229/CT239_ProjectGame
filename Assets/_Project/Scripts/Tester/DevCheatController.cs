using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Core;
using _Project.Scripts.Managers;

namespace _Project.Scripts.Tester
{
    /// <summary>
    /// Công cụ Debug Cheat hỗ trợ Tester / Developer kiểm thử nhanh:
    /// Tự động kích hoạt khi vào game (không cần phải kéo vào Scene).
    /// F1: Bật / Tắt bảng Debug HUD trên màn hình
    /// F2: Hack +1000$ vào ví người chơi
    /// F3: Tua kết thúc ngày ngay lập tức
    /// F4: Max cấp tất cả Nâng cấp (Thùng xe, Quỹ giờ)
    /// F7 (hoặc số 7): Nhảy thẳng tới Ngày 7 để kiểm tra Thắng / Thua
    /// </summary>
    public class DevCheatController : MonoBehaviour
    {
        [Header("Tùy chọn")]
        [SerializeField] private bool showDebugHUD = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (FindFirstObjectByType<DevCheatController>() == null)
            {
                GameObject go = new GameObject("[DevCheatController]");
                go.AddComponent<DevCheatController>();
                DontDestroyOnLoad(go);
                Debug.Log("<color=cyan><b>[DEV CHEAT] Đã tự động kích hoạt bộ phím tắt F1-F4!</b></color>");
            }
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // F1: Bật / Tắt Debug HUD
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                showDebugHUD = !showDebugHUD;
                Debug.Log($"[DEV CHEAT] Debug HUD: {(showDebugHUD ? "BẬT" : "TẮT")}");
            }

            // F2: Hack +1000$
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                CheatAddMoney(1000);
            }

            // F3: Tua kết thúc ngày ngay lập tức
            if (Keyboard.current.f3Key.wasPressedThisFrame)
            {
                CheatEndCurrentDay();
            }

            // F4: Nâng cấp max tất cả
            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                CheatMaxAllUpgrades();
            }

            // Số 7: Nhảy thẳng tới Ngày 7
            if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame)
            {
                CheatJumpToDay7();
            }
        }

        public void CheatAddMoney(int amount)
        {
            if (DayManager.Instance != null)
            {
                DayManager.Instance.PlayerGold += amount;
                Debug.Log($"<color=yellow><b>[DEV CHEAT] Đã cộng +${amount}! Tổng ví hiện tại: ${DayManager.Instance.PlayerGold}</b></color>");
            }
        }

        public void CheatEndCurrentDay()
        {
            if (DayManager.Instance != null)
            {
                Debug.Log("<color=orange><b>[DEV CHEAT] Tua kết thúc ngày ngay lập tức!</b></color>");
                DayManager.Instance.EndDay(isManual: true);
            }
        }

        public void CheatMaxAllUpgrades()
        {
            if (UpgradeManager.Instance == null) return;

            var available = UpgradeManager.Instance.AvailableUpgrades;
            if (available == null) return;

            foreach (var upg in available)
            {
                if (upg == null) continue;
                int currentLvl = UpgradeManager.Instance.GetUpgradeLevel(upg.UpgradeID);
                while (currentLvl < upg.MaxLevel)
                {
                    currentLvl++;
                    var record = UpgradeManager.Instance.GetPlayerUpgrade(upg.UpgradeID);
                    if (record != null)
                    {
                        record.CurrentLevel = currentLvl;
                    }
                    EventBus.Publish(new GameEvent.UpgradePurchasedEvent(
                        playerUpgradeId: record != null ? record.PlayerUpgradeID : upg.UpgradeID,
                        upgradeId: upg.UpgradeID,
                        newLevel: currentLvl,
                        costPaid: 0,
                        upgradeType: upg.UpgradeType
                    ));
                }
            }
            Debug.Log("<color=green><b>[DEV CHEAT] ĐÃ MỞ KHÓA TẤT CẢ NÂNG CẤP CẤP TỐI ĐA (Thùng xe, Quỹ giờ)!</b></color>");
        }

        public void CheatJumpToDay7()
        {
            if (DayManager.Instance != null)
            {
                DayManager.Instance.TestJumpToDay7();
            }
        }

        private void OnGUI()
        {
            if (!showDebugHUD) return;

            // Vẽ bảng Debug HUD góc trên bên trái
            GUI.Box(new Rect(10, 10, 320, 190), "<b>=== DEVELOPER DEBUG CHEAT (Nhóm 6) ===</b>");

            int currentDay = DayManager.Instance != null ? DayManager.Instance.CurrentDayIndex : 1;
            int gold = DayManager.Instance != null ? DayManager.Instance.PlayerGold : 0;
            int stars = DayManager.Instance != null ? DayManager.Instance.TotalReputationStars : 0;
            float timeRem = DayManager.Instance != null ? DayManager.Instance.TimeRemaining : 0f;

            GUI.Label(new Rect(20, 35, 300, 20), $"• Ngày hiện tại: Ngày {currentDay} / 7");
            GUI.Label(new Rect(20, 55, 300, 20), $"• Tiền ví người chơi: ${gold} (Mục tiêu: $5000)");
            GUI.Label(new Rect(20, 75, 300, 20), $"• Tổng sao uy tín: {stars} ⭐");
            GUI.Label(new Rect(20, 95, 300, 20), $"• Quỹ thời gian ngày: {timeRem:0.0}s");

            GUI.Label(new Rect(20, 120, 300, 20), "<b>[F1]</b>: Ẩn/Hiện HUD | <b>[F2]</b>: +$1000 Vàng");
            GUI.Label(new Rect(20, 140, 300, 20), "<b>[F3]</b>: Tua hết ngày | <b>[F4]</b>: Nâng cấp Max");
            GUI.Label(new Rect(20, 160, 300, 20), "<b>[Phím 7]</b>: Nhảy thẳng Ngày 7 test Thắng/Thua");
        }
    }
}
