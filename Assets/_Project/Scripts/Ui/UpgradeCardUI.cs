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

            int currentLevel = UpgradeManager.Instance.GetUpgradeLevel(currentData.UpgradeID);
            bool isMax = currentData.IsMaxLevel(currentLevel);
            
            //Cap nhat ten kem cap do
            if (txtName != null)
            {
                txtName.text = $"{currentData.UpgradeName} (Lv.{currentLevel + 1})";
            }
            
            // Bổ sung trạng thái Đạt Cấp Tối Đa (Max Level)
            if (isMax)
            {
                if (txtPrice != null) txtPrice.text = "MAX";
                if (btnBuy != null) btnBuy.interactable = false;
                if (txtBuyButton != null) txtBuyButton.text = "Đã tối đa";
                return;
            }

            //Van con nang cap duoc
            int nextCost = UpgradeManager.Instance.GetNextUpgradeCost(currentData.UpgradeID);
            if (txtPrice != null) txtPrice.text = $"${nextCost}";

            bool canAfford = currentGold >=  nextCost;
            if (btnBuy != null) btnBuy.interactable = canAfford;
            if (txtBuyButton != null) txtBuyButton.text = canAfford ? "Mua" : "Không đủ tiền";
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