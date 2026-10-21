
using System;
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

        [Header("Visibility Controller")]
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

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

            // Chỉ gọi hàm cập nhật lock state theo thời gian, không nạp lại text hay tính lại tiền
            foreach (var card in activeCardList)
            {
                if (card != null)
                {
                    card.UpdateLockStateWithTime(currentDayTimeRemaining);
                }
            }
        }

        /// <summary>
        /// Tự động ẩn Bảng việc khi người chơi chọn đơn
        /// </summary>
        private void OnJobSelected(GameEvent.JobSelectedEvent evt)
        {
            SetVisibility(false);
        }

        /// <summary>
        /// Hiển thị lại Bảng việc sau khi hoàn thành đơn và cập nhật danh sách
        /// </summary>
        private void OnContinueFromJobResult(GameEvent.ContinueFromJobResultEvent evt)
        {
            SetVisibility(true);

            if (evt.CompletedJob != null)
            {
                currentDailyJobs.Remove(evt.CompletedJob);
            }

            PopulateJobs(currentDailyJobs, evt.RemainingTime);
        }
        
        /// <summary>
        /// Ẩn/hiện Bảng việc an toàn bằng CanvasGroup (không ngắt vòng đời của GameObject)
        /// </summary>
        public void SetVisibility(bool visibility)
        {
            if(canvasGroup != null)
            {
                canvasGroup.alpha = visibility ? 1 : 0;
                canvasGroup.interactable = visibility;
                canvasGroup.blocksRaycasts = visibility;
            }

            else
            {
                //du phong
                gameObject.SetActive(visibility);
            }
        }
    }
}