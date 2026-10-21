using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using _Project.Scripts.Data;
using _Project.Scripts.Ui;

namespace _Project.Scripts.Tester
{
    public class JobBoardTester : MonoBehaviour
    {
        [Header("Target UI")]
        [SerializeField] private JobBoardUI jobBoardUI;

        [Header("Test Parameters")]
        [SerializeField] private float testDayTimeRemaining = 100f;

        private void Start()
        {
            RunTest();
        }

        [ContextMenu("Run Test Job Board")]
        public void RunTest()
        {
            if (jobBoardUI == null)
            {
                Debug.LogError("[JobBoardTester] Chưa gán JobBoardUI vào Inspector!");
                return;
            }

            // Tạo danh sách JobData giả lập (Mock Data)
            List<JobData> mockJobs = CreateMockJobList();

            // Nạp danh sách đơn hàng và áp dụng quy tắc khoá/mở
            jobBoardUI.PopulateJobs(mockJobs, testDayTimeRemaining);

            Debug.Log($"<color=yellow>[JobBoardTester]</color> Đã nạp thành công {mockJobs.Count} đơn test với quỹ thời gian {testDayTimeRemaining}s.");
        }

        private List<JobData> CreateMockJobList()
        {
            List<JobData> list = new List<JobData>();

            // Đơn 1: Dễ (TimeMin = 40s) -> Sẽ MỞ vì TimeRemaining (100s) >= 40s
            JobData job1 = ScriptableObject.CreateInstance<JobData>();
            SetJobProperty(job1, "CustomerName", "Khách A (Đơn dễ)");
            SetJobProperty(job1, "TimeMin", 40f);
            SetJobProperty(job1, "TimeMax", 60f);
            list.Add(job1);

            // Đơn 2: Trung bình (TimeMin = 90s) -> Sẽ MỞ vì TimeRemaining (100s) >= 90s
            JobData job2 = ScriptableObject.CreateInstance<JobData>();
            SetJobProperty(job2, "CustomerName", "Khách B (Đơn vừa)");
            SetJobProperty(job2, "TimeMin", 90f);
            SetJobProperty(job2, "TimeMax", 120f);
            list.Add(job2);

            // Đơn 3: Khó (TimeMin = 150s) -> Sẽ KHOÁ (Nút disabled) vì TimeRemaining (100s) < 150s
            JobData job3 = ScriptableObject.CreateInstance<JobData>();
            SetJobProperty(job3, "CustomerName", "Khách C (Đơn khó - Sẽ bị khoá)");
            SetJobProperty(job3, "TimeMin", 150f);
            SetJobProperty(job3, "TimeMax", 200f);
            list.Add(job3);

            return list;
        }

        /// <summary>
        /// Hàm Helper dùng Reflection để ghi đè giá trị vào các thuộc tính/field Read-Only của JobData
        /// </summary>
        private void SetJobProperty(JobData job, string propertyName, object value)
        {
            Type type = typeof(JobData);

            // 1. Thử ghi trực tiếp nếu có Setter
            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(job, value);
                return;
            }

            // 2. Thử tìm Backing Field hoặc private field tương ứng (customerName, _customerName, <CustomerName>k__BackingField...)
            string camelCase = char.ToLower(propertyName[0]) + propertyName.Substring(1);
            FieldInfo field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                           ?? type.GetField(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                           ?? type.GetField(camelCase, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                           ?? type.GetField("_" + camelCase, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(job, value);
            }
            else
            {
                Debug.LogWarning($"[JobBoardTester] Không tìm thấy field/property '{propertyName}' trên JobData.");
            }
        }
    }
}