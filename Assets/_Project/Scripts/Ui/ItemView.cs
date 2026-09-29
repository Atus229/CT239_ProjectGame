// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using TMPro;
// using _Project.Scripts.Core;
// using _Project.Scripts.Data;

// namespace _Project.Scripts.Ui
// {
//     [RequireComponent(typeof(CanvasGroup))]
//     public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//     {
//         [Header("UI References")]
//         [SerializeField] private Image itemIcon;
//         [SerializeField] private TextMeshProUGUI sizeText;

//         [Header("Item Data")]
//         [SerializeField] private ItemData itemData;
//         [SerializeField] private string itemName = "Sample Item";
//         [SerializeField] private Vector2Int itemSize = new Vector2Int(1, 1);

//         private CanvasGroup canvasGroup;
//         private Canvas mainCanvas;
//         private Vector3 startPosition;

//         private void Awake()
//         {
//             canvasGroup = GetComponent<CanvasGroup>();
//             mainCanvas = GetComponentInParent<Canvas>();
//         }

//         public void SetupItem(string name, Sprite icon, Vector2Int size)
//         {
//             itemName = name;
//             itemSize = size;
//             if (itemIcon != null) itemIcon.sprite = icon;
//             if (sizeText != null) sizeText.text = $"{size.x}x{size.y}";
//         }

//         // --- XỬ LÝ KÉO THẢ ---

//         public void OnBeginDrag(PointerEventData eventData)
//         {
//             // 1. Lưu lại vị trí ban đầu trong hàng chờ
//             startPosition = transform.position;

//             // 2. Tắt raycast để con trỏ chuột có thể đâm xuyên qua món hàng, 
//             // giúp các ô Grid bên dưới nhận diện được sự kiện Drop/Hover
//             canvasGroup.blocksRaycasts = false;
//             canvasGroup.alpha = 0.7f; // Làm mờ nhẹ khi kéo
//         }

//         public void OnDrag(PointerEventData eventData)
//         {
//             // Cập nhật vị trí món hàng theo con trỏ chuột
//             if (mainCanvas != null && mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
//             {
//                 RectTransformUtility.ScreenPointToWorldPointInRectangle(
//                     mainCanvas.transform as RectTransform,
//                     eventData.position,
//                     mainCanvas.worldCamera,
//                     out Vector3 worldPoint
//                 );
//                 transform.position = worldPoint;
//             }
//             else
//             {
//                 transform.position = eventData.position;
//             }
//         }

//         public void OnEndDrag(PointerEventData eventData)
//         {
//             // 1. Bật lại raycast và độ hiển thị ban đầu
//             canvasGroup.blocksRaycasts = true;
//             canvasGroup.alpha = 1.0f;

//             Vector3 dropPos = transform.position;

//             // 2. Bắn sự kiện qua EventBus
//             EventBus.Publish(new GameEvent.ItemPlacedEvent(itemData, dropPos, this));

//             // 3. Debug.Log kiểm tra thông tin
//             Debug.Log($"[DragDropManager] Đã thả món hàng: '{itemName}' tại vị trí World Position: {dropPos}");

//             // 4. Reset về vị trí ban đầu trong hàng chờ (startPosition)
//             transform.position = startPosition;
//         }
//     }
// }

// using UnityEngine;
// using UnityEngine.EventSystems;
// using _Project.Scripts.Core;
// using _Project.Scripts.Data;

// namespace _Project.Scripts.Ui
// {
//     public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//     {
//         [Header("Data References")]
//         [SerializeField] private ItemData itemData; // Hoặc thuộc tính Data của bạn
//         public ItemData Data => itemData;

//         private Vector3 startPosition;

//         public void OnBeginDrag(PointerEventData eventData)
//         {
//             startPosition = transform.position;
//         }

//         public void OnDrag(PointerEventData eventData)
//         {
//             transform.position = eventData.position;
//         }

//         public void OnEndDrag(PointerEventData eventData)
//         {
//             Vector3 dropPos = transform.position;

//             // Chỉnh sửa phát sự kiện ItemPlacedEvent với 3 tham số (Data, Position, this)
//             if (itemData != null)
//             {
//                 EventBus.Publish(new GameEvent.ItemPlacedEvent(itemData, dropPos, this));
//                 Debug.Log($"[ItemView] Đã thả món hàng: '{itemData.ItemName}' tại vị trí: {dropPos}");
//             }

//             // Quay về vị trí ban đầu trong hàng chờ
//             transform.position = startPosition;
//         }
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Sử dụng nếu UI hiển thị tên dùng TextMeshPro
using _Project.Scripts.Core;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI Component References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText; // Hoặc dùng public Text if không xài TMP

        [Header("Data Reference")]
        [SerializeField] private ItemData itemData;
        public ItemData Data => itemData;

        private Vector3 startPosition;

        private void Start()
        {
            // Nếu đã gán sẵn ItemData trong Inspector thì tự cập nhật UI lúc bắt đầu
            if (itemData != null)
            {
                SetupItem(itemData);
            }
        }

        /// <summary>
        /// Hàm nạp ScriptableObject ItemData vào ItemView để đổi Tên và Ảnh
        /// </summary>
        public void SetupItem(ItemData data)
        {
            this.itemData = data;

            if (itemData == null) return;

            // 1. Cập nhật tên hiển thị lên Canvas
            if (itemNameText != null)
            {
                itemNameText.text = itemData.ItemName;
            }

            // 2. Cập nhật ảnh hiển thị lên Canvas
            if (itemIcon != null && itemData.ItemIcon != null)
            {
                itemIcon.sprite = itemData.ItemIcon;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector3 dropPos = transform.position;

            if (itemData != null)
            {
                // Bắn sự kiện ItemPlacedEvent gồm 3 tham số: ItemData, vị trí thả, và this (ItemView)
                EventBus.Publish(new GameEvent.ItemPlacedEvent(itemData, dropPos, this));
                Debug.Log($"[ItemView] Thả món: '{itemData.ItemName}' tại: {dropPos}");
            }

            // Trở về vị trí ban đầu trong hàng chờ
            transform.position = startPosition;
        }
    }
}