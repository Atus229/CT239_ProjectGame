using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    /// <summary>
    /// Hộp thông tin chi tiết món đồ (Item Detail Info Box)
    /// Hiển thị thông số (Tên, Thời gian xử lý, Tiền công, Icon Dễ vỡ) khi người chơi rê chuột vào món đồ trên khay.
    /// </summary>
    public class ItemInfoBoxUI : MonoBehaviour
    {
        public static ItemInfoBoxUI Instance { get; private set; }

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI txtItemName;
        [SerializeField] private TextMeshProUGUI txtItemTime;
        [SerializeField] private TextMeshProUGUI txtItemValue;

        [Header("UI Image / Status References")]
        [SerializeField] private Image imgFragile;

        [Header("UI Panel Reference")]
        [SerializeField] private CanvasGroup canvasGroup;

        /// <summary>
        /// Gọi an toàn từ ngoài (dùng Unity Object operator != null thay vì C# ?. để tránh MissingReferenceException khi huỷ Scene)
        /// </summary>
        public static void ShowInfo(ItemData itemData)
        {
            if (Instance != null)
            {
                Instance.Show(itemData);
            }
        }

        public static void HideInfo()
        {
            if (Instance != null)
            {
                Instance.Hide();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (imgFragile != null)
            {
                imgFragile.gameObject.SetActive(false);
            }

            // Mặc định ẩn hộp thông tin
            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Hiển thị thông tin món đồ
        /// </summary>
        public void Show(ItemData itemData)
        {
            if (this == null || itemData == null)
            {
                Hide();
                return;
            }

            // Tên món đồ
            if (txtItemName != null)
            {
                txtItemName.text = !string.IsNullOrEmpty(itemData.ItemName) ? itemData.ItemName : "Vật phẩm";
            }

            // Thời gian xử lý (Handling Time)
            if (txtItemTime != null)
            {
                txtItemTime.text = $"<color=#3498DB>{itemData.HandlingTime:0.#}s</color>";
            }

            // Tiền công / Giá trị
            if (txtItemValue != null)
            {
                txtItemValue.text = $"<color=#2ECC71>+${itemData.Value}</color>";
            }

            // Hiển thị ảnh icon dễ vỡ nếu món đồ là hàng dễ vỡ (IsFragile)
            if (imgFragile != null)
            {
                imgFragile.gameObject.SetActive(itemData.IsFragile);
            }

            SetVisible(true);
        }

        /// <summary>
        /// Ẩn hộp thông tin
        /// </summary>
        public void Hide()
        {
            if (this == null) return;

            SetVisible(false);
            if (imgFragile != null)
            {
                imgFragile.gameObject.SetActive(false);
            }
        }

        private void SetVisible(bool isVisible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.blocksRaycasts = false; // Không cản trở click/drag chuột
                canvasGroup.interactable = false;
            }
            else
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}
