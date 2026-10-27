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

        /// <summary>
        /// Setup dữ liệu giả lập (Mock data) cho thẻ Nâng cấp
        /// </summary>
        public void SetupCardMock(string title, string description, int price, Sprite icon, int currentGold, Action onBuyCallback = null)
        {
            if (txtName != null) txtName.text = title;
            if (txtDescription != null) txtDescription.text = description;
            if (txtPrice != null) txtPrice.text = $"${price}";
            if (imgIcon != null && icon != null) imgIcon.sprite = icon;

            onBuyClickAction = onBuyCallback;

            // Kiểm tra khả năng mua (đủ tiền hay thiếu tiền)
            bool canAfford = currentGold >= price;
            SetBuyButtonState(canAfford);
        }

        /// <summary>
        /// Cập nhật trạng thái nút Mua (xám/tắt tương tác nếu thiếu tiền)
        /// </summary>
        public void SetBuyButtonState(bool canAfford)
        {
            if (btnBuy != null)
            {
                btnBuy.interactable = canAfford;
            }

            if (txtBuyButton != null)
            {
                txtBuyButton.text = canAfford ? "Mua" : "Không đủ tiền";
            }
        }

        private void OnBuyButtonClicked()
        {
            onBuyClickAction?.Invoke();
            Debug.Log($"<color=green>[UpgradeCardUI]</color> Đã bấm mua {txtName?.text}!");
        }
    }
}