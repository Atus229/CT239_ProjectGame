// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// using _Project.Scripts.Data;
// using _Project.Scripts.Core;

// namespace _Project.Scripts.Ui
// {
//     public class JobBoardUI : MonoBehaviour
//     {
//         [Header("Container & Prefab")]
//         [SerializeField] private Transform cardContainer; // Chứa VerticalLayoutGroup / ScrollView Content
//         [SerializeField] private JobCardUI cardPrefab;

//         [Header("Runtime State")]
//         private List<JobCardUI> activeCardList = new List<JobCardUI>();
//         private List<JobData> currentDailyJobs = new List<JobData>();
//         private float currentDayTimeRemaining;

//         private void OnEnable()
//         {
//             // Đăng ký nhận các sự kiện từ EventBus
//             EventBus.Subscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
//             EventBus.Subscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
//             EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
//         }

//         private void OnDisable()
//         {
//             // Hủy đăng ký sự kiện khi ẩn/hủy UI
//             EventBus.Unsubscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
//             EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
//             EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
//         }

//         /// <summary>
//         /// Nạp danh sách đơn hàng cho Bảng việc từ Manager / Controller
//         /// </summary>
//         public void PopulateJobs(List<JobData> dailyJobs, float dayTimeRemaining)
//         {
//             currentDailyJobs = dailyJobs ?? new List<JobData>();
//             currentDayTimeRemaining = dayTimeRemaining;

//             // 1. Dọn dẹp các thẻ đơn cũ trong container
//             ClearExistingCards();

//             // 2. Sinh mới các thẻ đơn theo danh sách JobData
//             foreach (var job in currentDailyJobs)
//             {
//                 if (job == null) continue;

//                 JobCardUI cardInstance = Instantiate(cardPrefab, cardContainer);
//                 cardInstance.SetData(job, currentDayTimeRemaining);
//                 activeCardList.Add(cardInstance);
//             }

//             // Ép layout làm mới lại vị trí ngay lập tức
//             if (cardContainer is RectTransform rectTransform)
//             {
//                 LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
//             }
//         }

//         /// <summary>
//         /// Xóa toàn bộ GameObject thẻ cũ trong container
//         /// </summary>
//         private void ClearExistingCards()
//         {
//             foreach (Transform child in cardContainer)
//             {
//                 Destroy(child.gameObject);
//             }
//             activeCardList.Clear();
//         }

//         /// <summary>
//         /// Xử lý cập nhật quỹ thời gian còn lại trong ngày
//         /// </summary>
//         private void OnDayTimeUpdated(GameEvent.DayTimeUpdatedEvent evt)
//         {
//             currentDayTimeRemaining = evt.RemainingTime;

//             // Cập nhật lại trạng thái khoá/mở cho tất cả các thẻ đang hiển thị
//             foreach (var card in activeCardList)
//             {
//                 if (card != null)
//                 {
//                     card.UpdateLockState(currentDayTimeRemaining);
//                 }
//             }
//         }

//         /// <summary>
//         /// Sự kiện người chơi chọn 1 đơn hàng: Tự động đóng Bảng việc
//         /// </summary>
//         private void OnJobSelected(GameEvent.JobSelectedEvent evt)
//         {
//             gameObject.SetActive(false);
//         }

//         /// <summary>
//         /// Sự kiện hoàn thành đơn hàng quay về: Bật lại Bảng việc và xóa đơn đã làm
//         /// </summary>
//         private void OnContinueFromJobResult(GameEvent.ContinueFromJobResultEvent evt)
//         {
//             // Bật lại Bảng việc
//             gameObject.SetActive(true);

//             // Loại bỏ đơn vừa hoàn thành khỏi danh sách công việc trong ngày
//             if (evt.CompletedJob != null)
//             {
//                 currentDailyJobs.Remove(evt.CompletedJob);
//             }

//             // Làm mới danh sách thẻ và cập nhật lại trạng thái theo quỹ thời gian còn lại
//             PopulateJobs(currentDailyJobs, evt.RemainingTime);
//         }
//     }
// }

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Scripts.Data;
using _Project.Scripts.Core;

namespace _Project.Scripts.Ui
{
    public class JobBoardUI : MonoBehaviour
    {
        [Header("Container & Prefab")]
        [SerializeField] private Transform cardContainer; 
        [SerializeField] private JobCardUI cardPrefab;

        [Header("Runtime State")]
        private List<JobCardUI> activeCardList = new List<JobCardUI>();
        private List<JobData> currentDailyJobs = new List<JobData>();
        private float currentDayTimeRemaining;

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
        }

        /// <summary>
        /// Nạp danh sách đơn hàng cho Bảng việc từ Manager
        /// </summary>
        public void PopulateJobs(List<JobData> dailyJobs, float dayTimeRemaining)
        {
            currentDailyJobs = dailyJobs ?? new List<JobData>();
            currentDayTimeRemaining = dayTimeRemaining;

            ClearExistingCards();

            foreach (var job in currentDailyJobs)
            {
                if (job == null) continue;

                JobCardUI cardInstance = Instantiate(cardPrefab, cardContainer);
                cardInstance.SetData(job, currentDayTimeRemaining);
                activeCardList.Add(cardInstance);
            }

            if (cardContainer is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        private void ClearExistingCards()
        {
            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }
            activeCardList.Clear();
        }

        /// <summary>
        /// Cập nhật trạng thái khóa/mở các thẻ khi quỹ thời gian thay đổi
        /// </summary>
        private void OnDayTimeUpdated(GameEvent.DayTimeUpdatedEvent evt)
        {
            currentDayTimeRemaining = evt.RemainingTime;

            // Gọi lại SetData để JobCardUI gốc tự tính lại logic locked
            for (int i = 0; i < activeCardList.Count && i < currentDailyJobs.Count; i++)
            {
                if (activeCardList[i] != null && currentDailyJobs[i] != null)
                {
                    activeCardList[i].SetData(currentDailyJobs[i], currentDayTimeRemaining);
                }
            }
        }

        /// <summary>
        /// Tự động ẩn Bảng việc khi người chơi chọn đơn
        /// </summary>
        private void OnJobSelected(GameEvent.JobSelectedEvent evt)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hiển thị lại Bảng việc sau khi hoàn thành đơn và cập nhật danh sách
        /// </summary>
        private void OnContinueFromJobResult(GameEvent.ContinueFromJobResultEvent evt)
        {
            gameObject.SetActive(true);

            if (evt.CompletedJob != null)
            {
                currentDailyJobs.Remove(evt.CompletedJob);
            }

            PopulateJobs(currentDailyJobs, evt.RemainingTime);
        }
    }
}