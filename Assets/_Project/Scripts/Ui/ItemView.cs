using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Ui
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(LayoutElement))]
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI Component References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemInfoText;

        [Header("Data Reference")]
        [SerializeField] private ItemData itemData;
        public ItemData Data => itemData;

        private bool[,] currentShape;
        public bool[,] CurrentShape => currentShape;

        private bool isPlacedOnGrid = false;
        public bool IsPlacedOnGrid
        {
            get => isPlacedOnGrid;
            set
            {
                isPlacedOnGrid = value;
                if (isPlacedOnGrid)
                {
                    HidePlaceHolder();
                }
            }
        }

        private void HidePlaceHolder()
        {
            if (sharedPlaceholder != null)
            {
                sharedPlaceholder.SetActive(false);
                sharedPlaceholder.transform.SetParent(null, false);
            }
        }

        public bool isPlaceOnGrid
        {
            get => IsPlacedOnGrid;
            set => IsPlacedOnGrid = value;
        }

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Canvas parentCanvas;

        private Transform originalParent;
        private int originalSiblingIndex;
        private Vector3 initialTrayPosition;
        private Vector2 defaultCardSize;
        
        //Dùng chung 1 placeholder tĩnh duy nhất cho toàn bộ itemview
        private static GameObject sharedPlaceholder;
        private static LayoutElement  sharedLayoutElement;

        private bool isDragging = false;
        private Vector2 localPointerOffset;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

            if (rectTransform != null)
            {
                rectTransform.pivot = Vector2.zero;
                defaultCardSize = rectTransform.sizeDelta;
            }

            if (itemIcon != null)
            {
                itemIcon.preserveAspect = true;
            }
        }

        private void Start()
        {
            initialTrayPosition = transform.position;
            if (itemData != null)
            {
                SetupItem(itemData);
            }
            SetTextVisible(!isPlacedOnGrid);
        }

        private void Update()
        {
            if (isDragging && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                RotateItem();
            }
        }

        private void OnDestroy()
        {
            HidePlaceHolder();
        }

        public Vector2 GetGridSize()
        {
            float cellSize = GridManager.Instance != null ? GridManager.Instance.CellSize : 100f;
            if (currentShape != null)
            {
                return new Vector2(currentShape.GetLength(1) * cellSize, currentShape.GetLength(0) * cellSize);
            }

            if (itemData != null)
            {
                return new Vector2(itemData.Width * cellSize, itemData.Height * cellSize);
            }

            return new Vector2(cellSize, cellSize);
        }

        public void SetupItem(ItemData data)
        {
            itemData = data;
            if (itemData == null) return;

            if (itemNameText != null)
            {
                itemNameText.text = itemData.ItemName;
            }

            if (itemIcon != null && itemData.ItemIcon != null)
            {
                itemIcon.sprite = itemData.ItemIcon;
            }

            if (itemInfoText != null)
            {
                itemInfoText.text = $"{itemData.HandlingTime:0}s - ${itemData.Value}";
            }

            currentShape = itemData.GetShapeMatrix();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = isPlacedOnGrid ? GetGridSize() : defaultCardSize;
            }

            SetTextVisible(!isPlacedOnGrid);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            SetTextVisible(false);

            if (!isPlacedOnGrid && transform.parent != null)
            {
                originalParent = transform.parent;
                originalSiblingIndex = transform.GetSiblingIndex();

                // Tái sử dụng placeholder tĩnh, chỉ khởi tạo đúng 1 lần đầu tiên trong suốt cả game
                if (sharedPlaceholder == null)
                {
                    sharedPlaceholder = new GameObject("Shared_Card_Placeholder", typeof(RectTransform), typeof(LayoutElement));
                    sharedLayoutElement = sharedPlaceholder.GetComponent<LayoutElement>();
                    sharedLayoutElement.flexibleWidth = 0;
                    sharedLayoutElement.flexibleHeight = 0;
                }
                // Đặt placeholder vào vị trí của món đồ đang bốc đi để giữ chỗ
                sharedPlaceholder.transform.SetParent(originalParent, false);
                sharedPlaceholder.transform.SetSiblingIndex(originalSiblingIndex);
                sharedLayoutElement.preferredWidth = defaultCardSize.x;
                sharedLayoutElement.preferredHeight = defaultCardSize.y;
                sharedPlaceholder.SetActive(true);
                
            }

            if (isPlacedOnGrid)
            {
                EventBus.Publish(new GameEvent.ItemRemovedEvent(itemData));
                isPlacedOnGrid = false;
            }

            Transform dragRoot = parentCanvas != null ? parentCanvas.transform : transform.root;
            if (transform.parent != dragRoot)
            {
                transform.SetParent(dragRoot, true);
            }

            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = GetGridSize();
            }

            RectTransform parentRect = transform.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 mouseLocalInParent))
            {
                localPointerOffset = (Vector2)rectTransform.localPosition - mouseLocalInParent;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform parentRect = transform.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 mouseLocalInParent))
            {
                rectTransform.localPosition = mouseLocalInParent + localPointerOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            canvasGroup.blocksRaycasts = true;

            if (itemData != null)
            {
                EventBus.Publish(new GameEvent.ItemDroppedEvent(itemData, transform.position, this, currentShape));
            }
        }

        public void RotateItem()
        {
            if (currentShape == null) return;

            int oldRows = currentShape.GetLength(0);
            int oldColumns = currentShape.GetLength(1);
            if (oldColumns == 0 || oldRows == 0) return;

            int newRows = oldColumns;
            int newColumns = oldRows;

            float cellSize = GridManager.Instance != null ? GridManager.Instance.CellSize : 100f;
            float oldHeight = oldRows * cellSize;

            // Xoay 90 độ cùng chiều kim đồng hồ quanh pivot (0, 0): (x, y) -> (oldHeight - y, x)
            // Giữ điểm con trỏ chuột cố định trên thẻ để không bị giật vị trí khi xoay
            Vector2 grabPointOnItem = -localPointerOffset;
            Vector2 newGrabPoint = new Vector2(oldHeight - grabPointOnItem.y, grabPointOnItem.x);
            Vector2 mouseLocalInParent = (Vector2)rectTransform.localPosition + grabPointOnItem;

            localPointerOffset = -newGrabPoint;
            rectTransform.localPosition = mouseLocalInParent - newGrabPoint;

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(newColumns * cellSize, newRows * cellSize);
            }

            if (itemIcon != null)
            {
                itemIcon.transform.Rotate(0f, 0f, -90f);
            }

            var rotatedShape = new bool[newRows, newColumns];
            for (int row = 0; row < oldRows; row++)
            {
                for (int col = 0; col < oldColumns; col++)
                {
                    int newRow = col;
                    int newColumn = oldRows - 1 - row;
                    rotatedShape[newRow, newColumn] = currentShape[row, col];
                }
            }

            currentShape = rotatedShape;
        }

        public void ReturnToTrayPosition()
        {
            HidePlaceHolder();

            if (originalParent != null)
            {
                transform.SetParent(originalParent, false);
    
                // Nếu index cũ còn hợp lệ thì xếp vào chỗ cũ, ngược lại thì đưa xuống cuối danh sách khay
                if (originalSiblingIndex >= 0 && originalSiblingIndex < originalParent.childCount)
                {
                    transform.SetSiblingIndex(originalSiblingIndex);
                }
                else
                {
                    transform.SetAsLastSibling();
                }
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.position = initialTrayPosition;
            }

            transform.rotation = Quaternion.identity;
            isPlacedOnGrid = false;

            if (itemData != null)
            {
                currentShape = itemData.GetShapeMatrix();
            }

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = defaultCardSize;
            }

            if (itemIcon != null)
            {
                itemIcon.transform.rotation = Quaternion.identity;
            }

            SetTextVisible(true);
        }

        private void SetTextVisible(bool isVisible)
        {
            if (itemNameText != null) itemNameText.gameObject.SetActive(isVisible);
            if (itemInfoText != null) itemInfoText.gameObject.SetActive(isVisible);
        }
    }
}
