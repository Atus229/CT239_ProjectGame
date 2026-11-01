using System.Collections.Generic;
using _Project.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Ui
{
    public class UpgradeShopUI : MonoBehaviour
    {
        [Header("Canvas Group Controller")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("UI Text Components")]
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private TextMeshProUGUI txtGold;

        [Header("Scroll Container & Prefab")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private UpgradeCardUI upgradeCardPrefab;

        [Header("Buttons")]
        [SerializeField] private Button btnClose;

        [Header("Mock Data Test Settings")]
        [SerializeField] private int playerGoldMock = 200; // Tiền giả lập để test

        private List<UpgradeCardUI> spawnedCards = new List<UpgradeCardUI>();

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (btnClose != null)
            {
                btnClose.onClick.AddListener(CloseShop);
            }
        }

        private void OnDestroy()
        {
            // Dọn dẹp sự kiện để tránh rò rỉ bộ nhớ
            if (btnClose != null)
            {
                btnClose.onClick.RemoveListener(CloseShop);
            }
        }

        private void Start()
        {
            UpdateGoldUI();
            OpenShop();
            GenerateUpgradeCards();
        }

        public void OpenShop()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public void CloseShop()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            Debug.Log("<color=yellow>[UpgradeShopUI]</color> Đã đóng Cửa hàng Nâng cấp.");
        }

        private void UpdateGoldUI()
        {
            if (txtGold != null)
            {
                txtGold.text = $"Vàng hiện có: ${playerGoldMock}";
            }
        }

        /// <summary>
        /// Sinh cac the nang cap tu danh muc co san trong UpgradeManager
        /// </summary>
        private void GenerateUpgradeCards()
        {
            ClearCards();

            if (UpgradeManager.Instance == null || upgradeCardPrefab == null || cardContainer == null)
            {
                return;
            }

            //Duyet toan bo danh muc nang cap co trong game
            foreach (var card in UpgradeManager.Instance.AvailableUpgrades)
            {
                if (card == null) return;
                
                //Tao card
                UpgradeCardUI cardInstance = Instantiate(upgradeCardPrefab, cardContainer);
                
                //Setup card va gan callback
                cardInstance.SetupCard(card, playerGoldMock, () =>
                {
                    //Goi thang vao Upgrade Manager de mua
                    if (UpgradeManager.Instance.TryPurchaseUpgrade(card.UpgradeID, ref playerGoldMock))
                    {
                        UpdateGoldUI();
                        RefreshAllCardsState();
                    }
                });
                
                spawnedCards.Add(cardInstance);
                
            }
            
            // Cập nhật lại Layout cho ScrollView
            if (cardContainer is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }
        

        /// <summary>
        /// Duyệt danh sách thẻ đã khởi tạo để cập nhật trạng thái, KHÔNG tạo lại (Destroy/Instantiate)
        /// </summary>
        private void RefreshAllCardsState()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null)
                {
                    card.UpdateCardVisual(playerGoldMock);
                }
            }
        }

        private void ClearCards()
        {
			if (cardContainer == null) return;
            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }
            spawnedCards.Clear();
        }
    }
}