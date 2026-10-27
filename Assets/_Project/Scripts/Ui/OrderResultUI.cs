using System;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Ui
{
    public class OrderResultUI : MonoBehaviour
    {
        [Header("Lego Components")]
        [SerializeField] private StarRatingUI starRating;
        [SerializeField] private ProgressBarUI capacityBar;

        [Header("Performance Texts")]
        [SerializeField] private TextMeshProUGUI txtTotalJobTime;
        [SerializeField] private TextMeshProUGUI txtTargetWindow;
        [SerializeField] private TextMeshProUGUI txtMoneyEarned;
        [SerializeField] private TextMeshProUGUI txtTimeRemaining;

        [Header("Customer Review")]
        [SerializeField] private TextMeshProUGUI txtReviewQuote;

        [Header("Buttons")]
        [SerializeField] private Button btnContinue;

        [Header("Dữ liệu Test thử trong Play Mode")]
        [SerializeField] private bool testInPlayMode = false;
        [SerializeField] private int testStars = 4;
        [SerializeField] private float testJobTime = 65f;
        [SerializeField] private float testTargetMin = 50f;
        [SerializeField] private float testTargetMax = 60f;
        [SerializeField] private int testMoney = 110;
        [SerializeField] private float testUsedCapacity = 75f;
        [SerializeField] private float testMaxCapacity = 100f;
        [SerializeField] private float testTimeRemaining = 166f;
        [SerializeField] private string testQuote = "Cảm ơn bạn, đồ đạc đến nơi an toàn!";

        public event Action OnContinueClicked;

        public bool TestInPlayMode
        {
            get => testInPlayMode;
            set => testInPlayMode = value;
        }

        public void SetTestMode(bool enabled)
        {
            testInPlayMode = enabled;
        }

        private void Start()
        {
            if (testInPlayMode)
            {
                DisplayResult(
                    testStars,
                    testJobTime,
                    testTargetMin,
                    testTargetMax,
                    testMoney,
                    testUsedCapacity,
                    testMaxCapacity,
                    testTimeRemaining,
                    testQuote
                );
            }
        }

        private void Awake()
        {
            if (btnContinue != null)
            {
                btnContinue.onClick.AddListener(OnContinueButtonPressed);
            }
        }

       
        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobCompletedEvent>(HandleJobCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobCompletedEvent>(HandleJobCompleted);
        }

        

        //Nạp toàn bộ dữ liệu kết quả đơn hàng vào giao diện
        public void DisplayResult(
            int stars, 
            float totalJobTime, 
            float targetMin, 
            float targetMax, 
            int moneyEarned, 
            float usedCapacity, 
            float maxCapacity, 
            float dayTimeRemaining, 
            string reviewQuote)
        {
            if (starRating != null) 
                starRating.SetStars(stars);

            if (txtTotalJobTime != null) 
                txtTotalJobTime.text = $"{totalJobTime:0}s";

            if (txtTargetWindow != null) 
                txtTargetWindow.text = $"{targetMin:0}–{targetMax:0}s";

            if (txtMoneyEarned != null) 
                txtMoneyEarned.text = $"+${moneyEarned}";

            if (txtTimeRemaining != null) 
                txtTimeRemaining.text = $"{dayTimeRemaining:0}s";

            if (capacityBar != null) 
                capacityBar.SetProgress(usedCapacity, maxCapacity, "Cargo bed capacity");

            if (txtReviewQuote != null) 
                txtReviewQuote.text = $"\"{reviewQuote}\"";

            gameObject.SetActive(true);
            transform.SetAsLastSibling(); // Luôn nổi lên trên cùng của Canvas khi hiển thị kết quả
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        private void HandleJobCompleted(GameEvent.JobCompletedEvent evt)
        {
            int stars = evt.result.Stars;
            int earnedGold = evt.result.EarnedGold;
            float totalJobTime = evt.result.TotalJobTime;
            float timeMin = evt.job != null ? evt.job.TimeMin : 0f;
            float timeMax = evt.job != null ? evt.job.TimeMax : 0f;
            
            //tinh quy thoi gian con lai trong ngay
            float dayTimeRemaining = 0f;
            if (DayManager.Instance != null)
            {
                dayTimeRemaining = Mathf.Max(0f, DayManager.Instance.TimeRemaining - totalJobTime);
            }

            if (starRating != null)
            {
                starRating.SetStars(stars);
            }
            
            //Dien du lieu vao nhan
            if (txtTotalJobTime != null)
                txtTotalJobTime.text = $"{totalJobTime:0}s";
            if (txtTargetWindow != null)
                txtTargetWindow.text = $"{timeMin:0}–{timeMax:0}s";
            if (txtMoneyEarned != null)
                txtMoneyEarned.text = $"+${earnedGold}";
            if (txtTimeRemaining != null)
                txtTimeRemaining.text = $"{dayTimeRemaining:0}s";
            
            gameObject.SetActive(true);
            transform.SetAsLastSibling(); // Luôn nổi lên trên cùng của Canvas khi hiển thị kết quả
        }
        
        private void OnContinueButtonPressed()
        {
            //An bang ket qua
            gameObject.SetActive(false);
            
            //Phat tin hieu len eventbus cho daymanager va jobboardui nhan dien
            EventBus.Publish(new GameEvent.ContinueFromJobResultEvent());
            
        }

    }
}