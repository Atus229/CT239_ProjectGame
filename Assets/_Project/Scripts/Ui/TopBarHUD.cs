using UnityEngine;
using TMPro;
using _Project.Scripts.Core;

namespace _Project.Scripts.Ui
{
    public class TopBarHUD : MonoBehaviour
    {
        [Header("UI Text References")]
        [Tooltip("Kéo Text (TMP) hiển thị Tiền ví người chơi vào đây (nếu có)")]
        [SerializeField] private TextMeshProUGUI txtGold;

        [Tooltip("Kéo Text (TMP) hiển thị Quỹ giờ vào đây (nếu có)")]
        [SerializeField] private TextMeshProUGUI txtDayTime;

        [Tooltip("Kéo Text (TMP) HeaderCustomerName vào đây")]
        [SerializeField] private TextMeshProUGUI txtCustomerName;

        [Tooltip("Kéo Text (TMP) txtDayInfo vào đây (Ví dụ: Ngày 1/7)")]
        [SerializeField] private TextMeshProUGUI txtDayInfo;

        [Tooltip("Kéo Text (TMP) RoundCoins vào đây (Ví dụ: 100 / 500$)")]
        [SerializeField] private TextMeshProUGUI txtOrderCoins;

        private void Awake()
        {
            // Tự động tìm đối tượng trên Scene nếu chưa kịp kéo vào Inspector
            if (txtCustomerName == null)
            {
                txtCustomerName = GameObject.Find("HeaderCustomerName")?.GetComponent<TextMeshProUGUI>();
            }

            if (txtDayInfo == null)
            {
                txtDayInfo = GameObject.Find("txtDayInfo")?.GetComponent<TextMeshProUGUI>();
            }

            if (txtOrderCoins == null)
            {
                var roundCoinsObj = GameObject.Find("RoundCoins");
                if (roundCoinsObj != null)
                {
                    txtOrderCoins = roundCoinsObj.GetComponent<TextMeshProUGUI>() ?? roundCoinsObj.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
            EventBus.Subscribe<GameEvent.ItemPlacedEvent>(OnItemPlaced);
            EventBus.Subscribe<GameEvent.ItemRemovedEvent>(OnItemRemoved);

            RefreshUI();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<GameEvent.DayTimeUpdatedEvent>(OnDayTimeUpdated);
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
            EventBus.Unsubscribe<GameEvent.ItemPlacedEvent>(OnItemPlaced);
            EventBus.Unsubscribe<GameEvent.ItemRemovedEvent>(OnItemRemoved);
        }

        private void Start()
        {
            RefreshUI();
        }

        private void OnDayStarted(GameEvent.DayStartedEvent evt)
        {
            if (txtDayTime != null) txtDayTime.text = $"{evt.dayTimeBudget:0}s";
            RefreshGold();
            UpdateDayInfo();
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
                txtCustomerName.text = $"Đơn: {evt.SelectedJob.CustomerName}";
            }
            UpdateOrderCoins();
        }

        private void OnContinueFromJobResult(GameEvent.ContinueFromJobResultEvent evt)
        {
            if (txtDayTime != null) txtDayTime.text = $"{evt.RemainingTime:0}s";
            RefreshGold();
            if (txtCustomerName != null) txtCustomerName.text = "";
            UpdateOrderCoins();
        }

        private void OnItemPlaced(GameEvent.ItemPlacedEvent evt)
        {
            UpdateOrderCoins();
        }

        private void OnItemRemoved(GameEvent.ItemRemovedEvent evt)
        {
            UpdateOrderCoins();
        }

        public void RefreshUI()
        {
            RefreshGold();
            RefreshTime();
            UpdateDayInfo();
            UpdateOrderCoins();
        }

        private void RefreshGold()
        {
            if (txtGold != null)
            {
                int gold = DayManager.Instance != null ? DayManager.Instance.PlayerGold : 0;
                txtGold.text = $"${gold} / $5000";
            }
        }

        private void RefreshTime()
        {
            if (txtDayTime != null && DayManager.Instance != null)
            {
                txtDayTime.text = $"{DayManager.Instance.TimeRemaining:0}s";
            }
        }

        public void UpdateDayInfo()
        {
            if (txtDayInfo != null)
            {
                int day = (DayManager.Instance != null && DayManager.Instance.CurrentDayIndex > 0)
                    ? DayManager.Instance.CurrentDayIndex
                    : 1;
                txtDayInfo.text = $"Ngày {day}/7";
            }
        }

        public void UpdateOrderCoins()
        {
            if (txtOrderCoins != null)
            {
                int currentVal = JobEconomyManager.Instance != null ? JobEconomyManager.Instance.CurrentJobValue : 0;
                int totalVal = JobEconomyManager.Instance != null ? JobEconomyManager.Instance.TotalJobValue : 0;
                txtOrderCoins.text = $"{currentVal} / {totalVal}$";
            }
        }
    }
}
