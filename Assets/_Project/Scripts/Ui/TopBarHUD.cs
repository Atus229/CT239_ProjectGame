using UnityEngine;
using TMPro;
using _Project.Scripts.Core;

namespace _Project.Scripts.Ui
{
    public class TopBarHUD : MonoBehaviour
    {
        [Header("UI Text References")]
        [Tooltip("Kéo Text (TMP) hiển thị Tiền vào đây")]
        [SerializeField] private TextMeshProUGUI txtGold;

        [Tooltip("Kéo Text (TMP) hiển thị Quỹ giờ vào đây")]
        [SerializeField] private TextMeshProUGUI txtDayTime;

        [Tooltip("Kéo Text (TMP) hiển thị Tên khách hàng (nếu có) vào đây")]
        [SerializeField] private TextMeshProUGUI txtCustomerName;

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);

            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
        }

        private void Start()
        {
            RefreshUI();
        }

        private void OnDayStarted(GameEvent.DayStartedEvent evt)
        {
            if (txtDayTime != null) txtDayTime.text = $"{evt.dayTimeBudget:0}s";
            RefreshGold();
        }

        private void OnDayTimeUpdated(GameEvent.DayTimeUpdatedEvent evt)
        {
            if (txtDayTime != null) txtDayTime.text = $"{evt.RemainingTime:0}s";
            RefreshGold();
        }

        private void OnJobSelected(GameEvent.JobSelectedEvent evt)
        {
            if (txtCustomerName != null && evt.SelectedJob != null)
            {
                txtCustomerName.text = $"{evt.SelectedJob.CustomerName}";
            }
        }

        private void OnContinueFromJobResult(GameEvent.ContinueFromJobResultEvent evt)
        {
            if (txtDayTime != null) txtDayTime.text = $"{evt.RemainingTime:0}s";
            RefreshGold();
            if (txtCustomerName != null) txtCustomerName.text = "";
        }

        public void RefreshUI()
        {
            RefreshGold();
            RefreshTime();
        }

        private void RefreshGold()
        {
            if (txtGold != null)
            {
                int gold = DayManager.Instance != null ? DayManager.Instance.PlayerGold : 0;
                txtGold.text = $"${gold}";
            }
        }

        private void RefreshTime()
        {
            if (txtDayTime != null && DayManager.Instance != null)
            {
                txtDayTime.text = $"{DayManager.Instance.TimeRemaining:0}s";
            }
        }
    }
}