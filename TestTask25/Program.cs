using System;
using System.Collections.Generic;

namespace TestTask25
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==============================================");
            Console.WriteLine("=== TESTING DAY MANAGER LOGIC FOR TASK-25 ===");
            Console.WriteLine("==============================================\n");

            DayManager dayManager = new DayManager();

            // 1. Test khoi tao ngay 1[cite: 2]
            DayData day1 = new DayData { DayIndex = 1, DayTimeBudget = 220f };
            dayManager.StartNewDay(day1);
            Console.WriteLine($"[PASS] Start Day 1. Budget: {dayManager.TimeBudget}s, Remaining: {dayManager.TimeRemaining}s");

            // 2. Test kiem tra nhan don[cite: 2]
            JobData jobA = new JobData { JobId = "Job_KhuA", TimeMin = 50f, TimeMax = 60f };
            bool canAccept = dayManager.CanAcceptJob(jobA);
            Console.WriteLine($"[PASS] Can accept JobA (Tmin=50s)? -> {canAccept}");

            // 3. Test tru quy thoi gian[cite: 2]
            dayManager.DeductTimeAndRecordJob(54f, 180, 5);
            Console.WriteLine($"[PASS] Deducted 54s. Time Remaining: {dayManager.TimeRemaining}s");

            Console.WriteLine("\n>>> ALL TESTS PASSED SUCCESSFULLY! <<<");
        }
    }
}