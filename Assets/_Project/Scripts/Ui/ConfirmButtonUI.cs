using System;
using _Project.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Ui
{
    [RequireComponent(typeof(Button))]
    public class ConfirmButtonUI : MonoBehaviour
    {
        private Button confirmButton;

        void Awake()
        {
            confirmButton = GetComponent<Button>();
            //False: la trang thai bi xam cua nut -> chua nhan duoc
            confirmButton.interactable = false;

            //Gan Gameobject de bat su kien click tren man hinh
            //Thay vi phai keo thu cong tren inspector
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobConfirmStateChangedEvent>(HandleConfirmStateChanged);
        }


        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobConfirmStateChangedEvent>(HandleConfirmStateChanged);
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

            JobResult result = JobEconomyManager.Instance.CalculateJobResult();

            // Bắn sự kiện ra toàn bộ hệ thống (DayManager sẽ tự nhận được)
            EventBus.Publish(new GameEvent.JobSubmittedEvent(null, result));

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
