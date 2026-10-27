using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Sử dụng nếu UI hiển thị tên dùng TextMeshPro
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Ui
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI Component References")] [SerializeField]
        private Image itemIcon;

        [SerializeField] private TextMeshProUGUI itemNameText; // Hoặc dùng public Text if không xài TMP
        [SerializeField] private TextMeshProUGUI itemInfoText; // Hiển thị Handling Time và Value (ví dụ: 12s - $40)

        [Header("Data Reference")] [SerializeField]
        private ItemData itemData;

        public ItemData Data => itemData;

        private bool[,] currentShape;
        public bool[,] CurrentShape => currentShape;

        private Vector3 initialTrayPosition;
        private Transform originalParent;
        private int originalSiblingIndex;
        private bool isPlacedOnGrid = false;

        public bool isPlaceOnGrid
        {
            get => isPlacedOnGrid;
            set => isPlacedOnGrid = value;
        }

        [Header("Queue Card Presentation")]
        [Tooltip("Kích thước đồng nhất của thẻ khi nằm trong khay chờ")]
        [SerializeField]
        private Vector2 queueCardSize = new Vector2(120f, 120f);

        public Vector2 QueueCardSize => queueCardSize;

        public void SetQueueCardSize(Vector2 size)
        {
            queueCardSize = size;
            if (!isPlacedOnGrid && !isDragging && rectTransform != null)
            {
                rectTransform.sizeDelta = queueCardSize;
            }
        }

        public Vector2 GetGridSize()
        {
            if (currentShape != null)
            {
                return new Vector2(currentShape.GetLength(1) * 100f, currentShape.GetLength(0) * 100f);
            }

            if (itemData != null)
            {
                return new Vector2(itemData.Width * 100f, itemData.Height * 100f);
            }

            return new Vector2(100f, 100f);
        }

        private float trayScale = 1f;

        public void SetTrayScale(float scale)
        {
            trayScale = scale;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private Vector3 startPosition;
        private Quaternion startRotation;
        private CanvasGroup canvasGroup;
        private bool isDragging = false;
        private Vector3 pointerOffset;
        private RectTransform rectTransform;
        private Canvas parentCanvas;

        private Vector2 localPointerOffset;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            if (rectTransform != null)
            {
                rectTransform.pivot = Vector2.zero;
            }

            if (itemIcon != null)
            {
                itemIcon.preserveAspect = true;
            }
        }

        private void Start()
        {
            initialTrayPosition = transform.position;
            // Nếu đã gán sẵn ItemData trong Inspector thì tự cập nhật UI lúc bắt đầu
            if (itemData != null)
            {
                SetupItem(itemData);
                if (transform.parent == null || transform.parent.GetComponent<LayoutElement>() == null)
                {
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = GetGridSize();
                    }
                }
            }
        }

        private void Update()
        {
            if (isDragging)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    RotateItem();
                }
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

            if (itemInfoText != null)
            {
                itemInfoText.text = $"{itemData.HandlingTime:0}s - ${itemData.Value}";
            }

            currentShape = itemData.GetShapeMatrix();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = queueCardSize;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            startPosition = transform.position;
            startRotation = transform.rotation;

            if (!isPlacedOnGrid)
            {
                originalParent = transform.parent;
                originalSiblingIndex = transform.GetSiblingIndex();
            }

            Transform dragContainer = GetComponentInParent<ItemQueueSpawner>()?.transform;
            if (dragContainer == null && parentCanvas != null)
            {
                dragContainer = parentCanvas.transform;
            }

            if (dragContainer != null && transform.parent != dragContainer)
            {
                transform.SetParent(dragContainer, true);
            }

            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;

            if (rectTransform != null)
            {
                rectTransform.sizeDelta = GetGridSize();
            }

            if (isPlacedOnGrid)
            {
                EventBus.Publish(new GameEvent.ItemRemovedEvent(itemData));
                isPlacedOnGrid = false;
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

            // Bám dính liên tục không độ trễ, không lệch hệ quy chiếu
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
            Vector3 dropPos = transform.position;

            if (itemData != null)
            {
                // Bắn sự kiện ItemPlacedEvent gồm 3 tham số: ItemData, vị trí thả, và this (ItemView)
                EventBus.Publish(new GameEvent.ItemDroppedEvent(itemData, dropPos, this, currentShape));
                Debug.Log($"[ItemView] Thả món: '{itemData.ItemName}' tại: {dropPos}");
            }
        }

        public void RotateItem()
        {
            if (currentShape == null)
            {
                return;
            }

            int oldRows = currentShape.GetLength(0); // lay rows
            int oldColumns = currentShape.GetLength(1); //lay columns

            if (oldColumns == 0 || oldRows == 0)
            {
                return;
            }

            //Gan kich thuoc cho ma tran moi
            int newRows = oldColumns;
            int newColumns = oldRows;

            // Cập nhật kích thước hiển thị của khung cha (Width <-> Height)
            // Đảm bảo Pivot (0, 0) luôn nằm ở góc Dưới - Trái của hình dáng mới
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(newColumns * 100f, newRows * 100f);
            }

            // Chỉ xoay hình ảnh con (itemIcon) để hiển thị trực quan
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
            if (originalParent != null && parentCanvas != null && originalParent != parentCanvas.transform)
            {
                transform.SetParent(originalParent, false);
                transform.SetSiblingIndex(originalSiblingIndex);
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.position = initialTrayPosition;
            }

            transform.rotation = Quaternion.identity;
            isPlacedOnGrid = false;
            if (itemData != null) currentShape = itemData.GetShapeMatrix();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = queueCardSize; // Trở lại kích thước thẻ bài đồng nhất trong khay
            }

            if (itemIcon != null) itemIcon.transform.rotation = Quaternion.identity;
        }
    }
}