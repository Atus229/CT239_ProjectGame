using System;
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
        [SerializeField] private bool testInPlayMode = true;
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
                btnContinue.onClick.AddListener(() => OnContinueClicked?.Invoke());
            }
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
            gameObject.SetActive(false);
        }
    }
}