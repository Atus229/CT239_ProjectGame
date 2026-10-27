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
        [SerializeField] private int playerGoldMock = 200; // Tiền giả lập để test (thẻ $500 sẽ bị mờ)

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
        /// Tạo 3 thẻ nâng cấp mẫu với dữ liệu Mock để kiểm tra Layout và Button Lock
        /// </summary>
        private void GenerateMockCards()
        {
            ClearCards();

            // Thẻ 1: Đủ tiền mua ($150)
            CreateCardMockInstance(
                "Mở rộng thùng xe",
                "Tăng thêm 1 cột ô chứa đồ trong thùng hàng.",
                150,
                null
            );

            // Thẻ 2: Đủ tiền mua ($100)
            CreateCardMockInstance(
                "Tăng tốc độ di chuyển",
                "Tốc độ di chuyển xe tải tăng thêm 15%.",
                100,
                null
            );

            // Thẻ 3: THIẾU TIỀN ($500 > Vàng hiện có $200)
            CreateCardMockInstance(
                "Bảo hiểm hàng hóa",
                "Giảm 50% thiệt hại tiền phạt khi làm hỏng đồ.",
                500,
                null
            );

            // Cập nhật lại Layout để ScrollView nhận diện chiều cao Content
            if (cardContainer is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }

        private void CreateCardMockInstance(string title, string description, int price, Sprite icon)
        {
            if (upgradeCardPrefab == null || cardContainer == null) return;

            UpgradeCardUI cardInstance = Instantiate(upgradeCardPrefab, cardContainer);
            cardInstance.SetupCardMock(title, description, price, icon, playerGoldMock, () =>
            {
                if (playerGoldMock >= price)
                {
                    playerGoldMock -= price;
                    UpdateGoldUI();
                    RefreshAllCardsState();
                }
            });

            spawnedCards.Add(cardInstance);
        }

        private void RefreshAllCardsState()
        {
            GenerateMockCards();
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