using System.Collections.Generic;
using _Project.Scripts.Core;
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

        [Header("Test & Mock Data Settings")]
        [Tooltip("Nếu tick, Shop sẽ tự mở khi nhấn Play (dùng cho Scene Test). Khi vào Game thật, hãy bỏ tick.")]
        [SerializeField] private bool openOnStart = false;
        [SerializeField] private int playerGoldMock = 200; // Tiền giả lập để test khi không có DayManager

        private int currentGold;
        private readonly List<UpgradeCardUI> spawnedCards = new List<UpgradeCardUI>();

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
            // Mặc định đóng Shop để không che màn hình khi vào Scene chơi game
            if (openOnStart)
            {
                OpenShop();
            }
            else
            {
                CloseShop();
            }
        }

        /// <summary>
        /// Mở Cửa hàng Nâng cấp, tự động đồng bộ số vàng thực tế từ DayManager
        /// Đảm bảo luôn nằm đè lên trên bảng Summary Day (Last Sibling của Canvas)
        /// </summary>
        public void OpenShop()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 1. Lấy số vàng thực tế từ DayManager (hoặc fallback về mock data khi test độc lập)
            currentGold = GetCurrentGold();

            // 2. Cập nhật giao diện tiền
            UpdateGoldUI();

            // 3. Khởi tạo danh sách thẻ hoặc làm mới trạng thái
            if (spawnedCards.Count == 0)
            {
                GenerateUpgradeCards();
            }
            else
            {
                RefreshAllCardsState();
            }

            // 4. Bật hiển thị giao diện và đưa lên trên cùng Canvas (nằm đè lên DaySummary)
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            transform.SetAsLastSibling();

            Debug.Log($"<color=cyan>[UpgradeShopUI]</color> Đã mở Cửa hàng Nâng cấp. Vàng khả dụng: ${currentGold}");
        }

        /// <summary>
        /// Đóng Cửa hàng Nâng cấp và ẩn khỏi màn hình
        /// </summary>
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
                txtGold.text = $"Vàng hiện có: ${currentGold}";
            }
        }

        /// <summary>
        /// Sinh các thẻ nâng cấp từ danh mục có sẵn trong UpgradeManager
        /// </summary>
        private void GenerateUpgradeCards()
        {
            ClearCards();

            if (UpgradeManager.Instance == null || upgradeCardPrefab == null || cardContainer == null)
            {
                return;
            }

            // Duyệt toàn bộ danh mục nâng cấp có trong game
            foreach (var card in UpgradeManager.Instance.AvailableUpgrades)
            {
                if (card == null) continue;
                
                // Tạo card
                UpgradeCardUI cardInstance = Instantiate(upgradeCardPrefab, cardContainer);
                
                // Setup card và gán callback
                cardInstance.SetupCard(card, currentGold, () =>
                {
                    // Gọi thẳng vào UpgradeManager để mua
                    if (UpgradeManager.Instance.TryPurchaseUpgrade(card.UpgradeID, ref currentGold))
                    {
                        SaveCurrentGold(currentGold);
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
                    card.UpdateCardVisual(currentGold);
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

        /// <summary>
        /// Lấy số vàng thực tế từ DayManager nếu có, hoặc dùng mock data
        /// </summary>
        private int GetCurrentGold()
        {
            if (DayManager.Instance != null)
            {
                return DayManager.Instance.PlayerGold;
            }
            return playerGoldMock;
        }

        /// <summary>
        /// Lưu lại số vàng sau khi mua nâng cấp vào DayManager hoặc mock data
        /// </summary>
        private void SaveCurrentGold(int newGold)
        {
            currentGold = newGold;
            if (DayManager.Instance != null)
            {
                DayManager.Instance.PlayerGold = newGold;
            }
            else
            {
                playerGoldMock = newGold;
            }
        }
    }
}
