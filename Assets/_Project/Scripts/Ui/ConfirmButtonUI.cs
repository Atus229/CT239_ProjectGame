using System;
using _Project.Scripts.Core;
using _Project.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    [RequireComponent(typeof(Button))]
    public class ConfirmButtonUI : MonoBehaviour
    {
        private Button confirmButton;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            confirmButton = GetComponent<Button>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Trạng thái bị xám ban đầu của nút -> chưa bấm được
            confirmButton.interactable = false;

            // Gán listener bắt sự kiện click
            confirmButton.onClick.AddListener(OnConfirmClicked);

            // Ban đầu chưa chọn đơn -> ẩn nút xác nhận
            //SetVisibility(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobConfirmStateChangedEvent>(HandleConfirmStateChanged);
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
            EventBus.Subscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Subscribe<GameEvent.DayEndedEvent>(HandleDayEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobConfirmStateChangedEvent>(HandleConfirmStateChanged);
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(HandleJobSelected);
            EventBus.Unsubscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
            EventBus.Unsubscribe<GameEvent.DayEndedEvent>(HandleDayEnded);
        }

        private void HandleJobSelected(GameEvent.JobSelectedEvent e)
        {
            SetVisibility(true);
            if (confirmButton != null) confirmButton.interactable = false;
        }

        private void HandleJobSubmitted(GameEvent.JobSubmittedEvent e)
        {
            SetVisibility(false);
        }

        private void HandleDayEnded(GameEvent.DayEndedEvent e)
        {
            SetVisibility(false);
        }

        private void SetVisibility(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
        }

        private void HandleConfirmStateChanged(GameEvent.JobConfirmStateChangedEvent e)
        {
            if (confirmButton == null) return;

            confirmButton.interactable = e.CanConfirm;
        }

        private void OnConfirmClicked()
        {
            if (JobEconomyManager.Instance == null)
            {
                Debug.Log("[ConfirmButtonUI] Không tìm thấy JobEconomyManager trong Scene!");
                return;
            }

            AudioManager.Instance?.PlayTruckHorn();

            confirmButton.interactable = false;
            
            JobResult result = JobEconomyManager.Instance.CalculateJobResult();

            // Bắn sự kiện ra toàn bộ hệ thống (DayManager và UI sẽ tự xử lý qua EventBus)
            EventBus.Publish(new GameEvent.JobSubmittedEvent(JobEconomyManager.Instance.CurrentJobData, result));

            // In kết quả ra màn hình Console với màu sắc trực quan
            string statusText = result.IsOnTime ? "<color=#00FF00>ĐÚNG HẠN</color>" : "<color=#FF0000>TRỄ HẠN</color>";
            Debug.Log($"<b>[XÁC NHẬN VẬN CHUYỂN THÀNH CÔNG]</b>\n" +
                      $"Trạng thái: {statusText}\n" +
                      $"Tổng thời gian: <b>{result.TotalJobTime:F1}s</b>\n" +
                      $"Đánh giá: <b>{result.Stars} SAO ⭐</b>\n" +
                      $"Tiền thưởng nhận được: <b>{result.EarnedGold} VÀNG 💰</b>");
        }
    }
}
