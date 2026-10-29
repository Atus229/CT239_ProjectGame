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

        [SerializeField] private CanvasGroup canvasGroup;
        
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
            if(canvasGroup==null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if (btnContinue != null)
            {
                btnContinue.onClick.AddListener(OnContinueButtonPressed);
            }

            SetVisibility(false);
        }

        private void SetVisibility(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }


        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSubmittedEvent>(HandleJobSubmitted);
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
        }

        public void Close()
        {
            SetVisibility(false);
        }
        
        private void HandleJobSubmitted(GameEvent.JobSubmittedEvent evt)
        {
            int stars = evt.JobResult.Stars;
            int earnedGold = evt.JobResult.EarnedGold;
            float totalJobTime = evt.JobResult.TotalJobTime;
            float timeMin = evt.Job != null ? evt.Job.TimeMin : 0f;
            float timeMax = evt.Job != null ? evt.Job.TimeMax : 0f;
            
            //tinh quy thoi gian con lai trong ngay
            float dayTimeRemaining = 0f;
            if (DayManager.Instance != null)
            {
                dayTimeRemaining = DayManager.Instance.TimeRemaining;
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
            
            SetVisibility(true);
        }
        
        private void OnContinueButtonPressed()
        {
            //An bang ket qua
            SetVisibility(false);
            
            JobData currentJob = JobEconomyManager.Instance != null ? JobEconomyManager.Instance.CurrentJobData : null;
            float timeRemaining = DayManager.Instance != null ? DayManager.Instance.TimeRemaining : 0f;
            
            //Phat tin hieu len eventbus cho daymanager va jobboardui nhan dien
            EventBus.Publish(new GameEvent.ContinueFromJobResultEvent(currentJob, timeRemaining));
            
        }

    }
}