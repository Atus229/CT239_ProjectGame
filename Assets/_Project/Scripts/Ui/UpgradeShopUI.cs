using System.Collections.Generic;
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
            // Yêu cầu 4: Dọn dẹp sự kiện để tránh rò rỉ bộ nhớ
            if (btnClose != null)
            {
                btnClose.onClick.RemoveListener(CloseShop);
            }
        }

        private void Start()
        {
            UpdateGoldUI();
            OpenShop();
            GenerateMockCards();
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
        /// Tạo danh sách thẻ nâng cấp mẫu ban đầu
        /// </summary>
        private void GenerateMockCards()
        {
            ClearCards();

            // Thẻ 1: Đủ tiền mua ($150)
            CreateCardMockInstance(
                "Mở rộng thùng xe",
                "Tăng thêm 1 cột ô chứa đồ trong thùng hàng.",
                150,
                null,
                false
            );

            // Thẻ 2: Đủ tiền mua ($100)
            CreateCardMockInstance(
                "Tăng tốc độ di chuyển",
                "Tốc độ di chuyển xe tải tăng thêm 15%.",
                100,
                null,
                false
            );

            // Thẻ 3: THIẾU TIỀN ($500 > Vàng hiện có $200)
            CreateCardMockInstance(
                "Bảo hiểm hàng hóa",
                "Giảm 50% thiệt hại tiền phạt khi làm hỏng đồ.",
                500,
                null,
                false
            );

            // Thẻ 4: ĐÃ ĐẠT CẤP TỐI ĐA (Test Max Level)
            CreateCardMockInstance(
                "Bình xăng phụ",
                "Đã nâng cấp đến cấp độ tối đa.",
                0,
                null,
                true
            );

            // Cập nhật lại Layout cho ScrollView
            if (cardContainer is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        private void CreateCardMockInstance(string title, string description, int price, Sprite icon, bool isMaxLevel = false)
        {
            if (upgradeCardPrefab == null || cardContainer == null) return;

            UpgradeCardUI cardInstance = Instantiate(upgradeCardPrefab, cardContainer);
            cardInstance.SetupCardMock(title, description, price, icon, playerGoldMock, isMaxLevel, () =>
            {
                if (playerGoldMock >= price && !isMaxLevel)
                {
                    playerGoldMock -= price;
                    UpdateGoldUI();
                    RefreshAllCardsState();
                }
            });

            spawnedCards.Add(cardInstance);
        }

        /// <summary>
        /// Yêu cầu 1: Duyệt danh sách thẻ đã khởi tạo để cập nhật trạng thái, KHÔNG tạo lại (Destroy/Instantiate)
        /// </summary>
        private void RefreshAllCardsState()
        {
            foreach (var card in spawnedCards)
            {
                if (card != null)
                {
                    card.UpdateAffordability(playerGoldMock);
                }
            }
        }

        private void ClearCards()
        {
            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }
            spawnedCards.Clear();
        }
    }
}