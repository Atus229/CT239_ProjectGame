using System;
using _Project.Scripts.Data;
using _Project.Scripts.Managers;
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
        private UpgradeData currentData;        
        private Color originalTextColor = Color.white;
        private bool hasCachedColor = false;

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

            CacheOriginalColor();
        }

        private void CacheOriginalColor()
        {
            if (hasCachedColor) return;
            if (txtBuyButton != null)
            {
                originalTextColor = txtBuyButton.color;
                hasCachedColor = true;
            }
            else if (txtPrice != null)
            {
                originalTextColor = txtPrice.color;
                hasCachedColor = true;
            }
        }

        private void OnDestroy()
        {
            // Dọn dẹp sự kiện để tránh rò rỉ bộ nhớ
            if (btnBuy != null)
            {
                btnBuy.onClick.RemoveListener(OnBuyButtonClicked);
            }
        }

        /// <summary>
        /// Setup dữ liệu giả lập (Mock data) cho thẻ Nâng cấp
        /// </summary>
        public void SetupCard(UpgradeData data, int currentGold, Action onBuyCallback = null)
        {
            currentData = data;
            onBuyClickAction = onBuyCallback;

            if (currentData == null) return;            

            // Xử lý Icon bị Null -> Ẩn GameObject Icon tránh hiển thị ô vuông màu trắng mặc định
            if (imgIcon != null)
            {
                if (currentData.Icon != null)
                {
                    imgIcon.sprite = currentData.Icon;
                    imgIcon.gameObject.SetActive(true);
                }
                else
                {
                    imgIcon.gameObject.SetActive(false);
                }
            }

            if(txtDescription != null) txtDescription.text = currentData.Description;

            UpdateCardVisual(currentGold);
        }

        /// <summary>
        /// Cập nhật lại trạng thái nút bấm và giá tiền mà không cần Destroy / Instantiate lại thẻ
        /// </summary>
        public void UpdateCardVisual(int currentGold)
        {
            if(currentData == null || UpgradeManager.Instance == null) return;
            CacheOriginalColor();

            int currentLevel = UpgradeManager.Instance.GetUpgradeLevel(currentData.UpgradeID);
            bool isMax = currentData.IsMaxLevel(currentLevel);
            
            // Cập nhật tên kèm cấp độ
            if (txtName != null)
            {
                txtName.text = $"{currentData.UpgradeName} (Lv.{currentLevel + 1})";
            }
            
            // Bổ sung trạng thái Đạt Cấp Tối Đa (Max Level)
            if (isMax)
            {
                if (txtPrice != null) txtPrice.text = "MAX";
                if (btnBuy != null) btnBuy.interactable = false;
                if (txtBuyButton != null)
                {
                    txtBuyButton.text = "MAX";
                    txtBuyButton.color = originalTextColor;
                }
                return;
            }

            // Vẫn còn nâng cấp được
            int nextCost = UpgradeManager.Instance.GetNextUpgradeCost(currentData.UpgradeID);
            string priceStr = $"${nextCost}";

            if (txtPrice != null) txtPrice.text = priceStr;
            if (txtBuyButton != null) txtBuyButton.text = priceStr;

            bool canAfford = currentGold >= nextCost;
            if (btnBuy != null) btnBuy.interactable = canAfford;

            // Luôn giữ nguyên màu chữ gốc 100% rõ nét, không làm mờ/chìm chữ dù nút bị xám
            if (txtBuyButton != null) txtBuyButton.color = originalTextColor;
            if (txtPrice != null && txtPrice != txtBuyButton) txtPrice.color = originalTextColor;
        }

        private void OnBuyButtonClicked()
        {
            // Chống Spam Click - Tắt tương tác ngay lập tức khi bấm
            if (btnBuy != null)
            {
                btnBuy.interactable = false;
            }

            onBuyClickAction?.Invoke();
            Debug.Log($"<color=green>[UpgradeCardUI]</color> Đã bấm mua {txtName?.text}!");
        }
    }
}