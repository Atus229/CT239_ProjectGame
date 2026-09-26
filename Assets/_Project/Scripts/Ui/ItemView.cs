using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using _Project.Scripts.Core;

namespace _Project.Scripts.Ui
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI sizeText;

        [Header("Item Data")]
        [SerializeField] private string itemName = "Sample Item";
        [SerializeField] private Vector2Int itemSize = new Vector2Int(1, 1);

        private CanvasGroup canvasGroup;
        private Canvas mainCanvas;
        private Vector3 startPosition;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            mainCanvas = GetComponentInParent<Canvas>();
        }

        public void SetupItem(string name, Sprite icon, Vector2Int size)
        {
            itemName = name;
            itemSize = size;
            if (itemIcon != null) itemIcon.sprite = icon;
            if (sizeText != null) sizeText.text = $"{size.x}x{size.y}";
        }

        // --- XỬ LÝ KÉO THẢ ---

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 1. Lưu lại vị trí ban đầu trong hàng chờ
            startPosition = transform.position;

            // 2. Tắt raycast để con trỏ chuột có thể đâm xuyên qua món hàng, 
            // giúp các ô Grid bên dưới nhận diện được sự kiện Drop/Hover
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f; // Làm mờ nhẹ khi kéo
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Cập nhật vị trí món hàng theo con trỏ chuột
            if (mainCanvas != null && mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    mainCanvas.transform as RectTransform,
                    eventData.position,
                    mainCanvas.worldCamera,
                    out Vector3 worldPoint
                );
                transform.position = worldPoint;
            }
            else
            {
                transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 1. Bật lại raycast và độ hiển thị ban đầu
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1.0f;

            Vector3 dropPos = transform.position;

            // 2. Bắn sự kiện qua EventBus
            EventBus.Publish(new ItemDroppedEvent(itemName, dropPos));

            // 3. Debug.Log kiểm tra thông tin
            Debug.Log($"[DragDropManager] Đã thả món hàng: '{itemName}' tại vị trí World Position: {dropPos}");

            // 4. Reset về vị trí ban đầu trong hàng chờ (startPosition)
            transform.position = startPosition;
        }
    }
}