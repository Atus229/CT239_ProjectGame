/*using UnityEngine;
using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Data;

namespace _Project.Scripts.Tester
{
    public class DayManagerTestRunner : MonoBehaviour
    {
        [Header("Tùy chọn")]
        [SerializeField] private bool runOnStart = true;

        private void Start()
        {
            if (runOnStart) RunAllTests();
        }

        [ContextMenu("Chạy Kiểm Thử DayManager")]
        public void RunAllTests()
        {
            Debug.Log("<color=#00E5FF><b>========== BẮT ĐẦU KIỂM THỬ DAY MANAGER (TASK 25) ==========</b></color>");

            DayManager dayMgr = FindFirstObjectByType<DayManager>();
            if (dayMgr == null)
            {
                GameObject go = new GameObject("[Test] DayManager");
                dayMgr = go.AddComponent<DayManager>();
            }

            // Thiết lập DayData giả lập: Ngân sách 120s
            DayData mockDay = ScriptableObject.CreateInstance<DayData>();
            mockDay.DayIndex = 1;
            mockDay.DayTimeBudget = 120f;
            dayMgr.StartNewDay(mockDay);

            Test_1_DeductTimeOnTime(dayMgr);
            Test_2_DeductTimeLate(dayMgr);
            Test_3_CanAcceptJobBoundary(dayMgr);
            Test_4_CheckAutoEndDay(dayMgr);

            Debug.Log("<color=#00E5FF><b>========== KẾT THÚC KIỂM THỬ DAY MANAGER ==========</b></color>");
        }

        private void Test_1_DeductTimeOnTimeOnTime(DayManager dayMgr)
        {
            // Reset quỹ về 120s
            DayData d = ScriptableObject.CreateInstance<DayData>();
            d.DayTimeBudget = 120f;
            dayMgr.StartNewDay(d);

            // Giao đúng hạn: Job 30s
            dayMgr.DeductTimeAndRecordJob(30f, 100, 5);
            AssertFloat("Case 1: Trừ quỹ đơn đúng hạn (120 - 30 = 90)", dayMgr.TimeRemaining, 90f);
        }

        private void Test_2_DeductTimeLate(DayManager dayMgr)
        {
            // Đơn bị trễ 75s (vượt Tmax) -> vẫn trừ đúng thời gian thực tế 75s
            dayMgr.DeductTimeAndRecordJob(75f, 50, 2);
            // 90s trước đó - 75s = 15s
            AssertFloat("Case 2: Trừ quỹ đơn trễ hạn (90 - 75 = 15)", dayMgr.TimeRemaining, 15f);

            // Cho phép trừ âm ở đơn cuối: 15s - 25s = -10s
            dayMgr.DeductTimeAndRecordJob(25f, 20, 1);
            AssertFloat("Case 2 (Biên): Cho phép âm tạm thời khi kết thúc đơn cuối (15 - 25 = -10)", dayMgr.TimeRemaining, -10f);
        }

        private void Test_3_CanAcceptJobBoundary(DayManager dayMgr)
        {
            DayData d = ScriptableObject.CreateInstance<DayData>();
            d.DayTimeBudget = 40f;
            dayMgr.StartNewDay(d); // TimeRemaining = 40f

            JobData job40s = ScriptableObject.CreateInstance<JobData>();
            // Gán giá trị timeMin = 40f
            var timeMinField = typeof(JobData).GetField("timeMin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timeMinField?.SetValue(job40s, 40f);

            // Điểm biên 1: DayTimeRemaining == job.TimeMin (40 == 40) -> True
            bool pass1 = dayMgr.CanAcceptJob(job40s) == true;
            LogResult("Case 3.1: Biên bằng nhau (Remaining 40s == Tmin 40s)", pass1);

            // Điểm biên 2: Giảm Remaining xuống 39s -> DayTimeRemaining < job.TimeMin -> False
            dayMgr.DeductTimeAndRecordJob(1f, 0, 0); // Remaining = 39s
            bool pass2 = dayMgr.CanAcceptJob(job40s) == false;
            LogResult("Case 3.2: Thiếu 1s (Remaining 39s < Tmin 40s)", pass2);
        }

        private void Test_4_CheckAutoEndDay(DayManager dayMgr)
        {
            bool dayEndedInvoked = false;
            dayMgr.OnDayEnded += () => dayEndedInvoked = true;

            // Còn 39s nhưng bảng việc chỉ có đơn đòi hỏi 50s
            JobData heavyJob = ScriptableObject.CreateInstance<JobData>();
            var timeMinField = typeof(JobData).GetField("timeMin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timeMinField?.SetValue(heavyJob, 50f);

            List<JobData> board = new List<JobData> { heavyJob };
            dayMgr.CheckAutoEndDayCondition(board);

            LogResult("Case 4: Tự động kết thúc ngày khi không còn đơn nào đủ điều kiện", dayEndedInvoked);
        }

        private void AssertFloat(string title, float actual, float expected)
        {
            if (Mathf.Approximately(actual, expected))
                Debug.Log($"<color=#4CAF50>✔ [PASS]</color> {title} => Thực tế: {actual}");
            else
                Debug.LogError($"<color=#F44336>✘ [FAIL]</color> {title} => Thực tế: {actual} (Kỳ vọng: {expected})");
        }

        private void LogResult(string title, bool isSuccess)
        {
            if (isSuccess)
                Debug.Log($"<color=#4CAF50>✔ [PASS]</color> {title}");
            else
                Debug.LogError($"<color=#F44336>✘ [FAIL]</color> {title}");
        }
    }
}*/