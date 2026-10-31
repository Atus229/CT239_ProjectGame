using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Ui
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image imgIcon;
        [SerializeField] private TextMeshProUGUI txtName;
        [SerializeField] private TextMeshProUGUI txtDescription;
        [SerializeField] private TextMeshProUGUI txtPrice;
        [SerializeField] private Button btnBuy;
        [SerializeField] private TextMeshProUGUI txtBuyButton;

        private Action onBuyClickAction;
        private int cardPrice;
        private bool isMaxLevel;

        private void Awake()
        {
            if (btnBuy != null)
            {
                btnBuy.onClick.AddListener(OnBuyButtonClicked);
            }

            if (txtBuyButton == null && btnBuy != null)
            {
                txtBuyButton = btnBuy.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void OnDestroy()
        {
            // Yêu cầu 4: Dọn dẹp sự kiện để tránh rò rỉ bộ nhớ
            if (btnBuy != null)
            {
                btnBuy.onClick.RemoveListener(OnBuyButtonClicked);
            }
        }

        /// <summary>
        /// Setup dữ liệu giả lập (Mock data) cho thẻ Nâng cấp
        /// </summary>
        public void SetupCardMock(string title, string description, int price, Sprite icon, int currentGold, bool isMaxLevel = false, Action onBuyCallback = null)
        {
            cardPrice = price;
            this.isMaxLevel = isMaxLevel;
            onBuyClickAction = onBuyCallback;

            if (txtName != null) txtName.text = title;
            if (txtDescription != null) txtDescription.text = description;

            // Yêu cầu 3: Xử lý Icon bị Null -> Ẩn GameObject Icon tránh hiển thị ô vuông màu trắng mặc định
            if (imgIcon != null)
            {
                if (icon != null)
                {
                    imgIcon.sprite = icon;
                    imgIcon.gameObject.SetActive(true);
                }
                else
                {
                    imgIcon.gameObject.SetActive(false);
                }
            }

            UpdateAffordability(currentGold);
        }

        /// <summary>
        /// Yêu cầu 1 & 2: Cập nhật lại trạng thái nút bấm và giá tiền mà không cần Destroy / Instantiate lại thẻ
        /// </summary>
        public void UpdateAffordability(int currentGold)
        {
            // Yêu cầu 2: Bổ sung trạng thái Đạt Cấp Tối Đa (Max Level)
            if (isMaxLevel)
            {
                if (txtPrice != null) txtPrice.text = "MAX";
                if (btnBuy != null) btnBuy.interactable = false;
                if (txtBuyButton != null) txtBuyButton.text = "Đã tối đa";
                return;
            }

            if (txtPrice != null) txtPrice.text = $"${cardPrice}";

            bool canAfford = currentGold >= cardPrice;
            if (btnBuy != null) btnBuy.interactable = canAfford;
            if (txtBuyButton != null) txtBuyButton.text = canAfford ? "Mua" : "Không đủ tiền";
        }

        private void OnBuyButtonClicked()
        {
            // Yêu cầu 3: Chống Spam Click - Tắt tương tác ngay lập tức khi bấm
            if (btnBuy != null)
            {
                btnBuy.interactable = false;
            }

            onBuyClickAction?.Invoke();
            Debug.Log($"<color=green>[UpgradeCardUI]</color> Đã bấm mua {txtName?.text}!");
        }
    }
}