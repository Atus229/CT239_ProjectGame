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
        [Header("UI Component References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText; // Hoặc dùng public Text if không xài TMP

        [Header("Data Reference")]
        [SerializeField] private ItemData itemData;
        public ItemData Data => itemData;

        private bool[,] currentShape;
        public bool[,] CurrentShape => currentShape;

        private Vector3 initialTrayPosition;
        private bool isPlacedOnGrid = false;
        public bool isPlaceOnGrid { get => isPlacedOnGrid; set => isPlacedOnGrid = value; } 
            
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
        }

        private void Start()
        {
            initialTrayPosition = transform.position;
            // Nếu đã gán sẵn ItemData trong Inspector thì tự cập nhật UI lúc bắt đầu
            if (itemData != null)
            {
                SetupItem(itemData);
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

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromResult);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromResult);
        }

        private void OnContinueFromResult(GameEvent.ContinueFromJobResultEvent e)
        {
            ReturnToTrayPosition();
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
            
            currentShape = itemData.GetShapeMatrix();
            if (currentShape != null && rectTransform != null)
            {
                int rows = currentShape.GetLength(0);
                int cols = currentShape.GetLength(1);
                
                rectTransform.sizeDelta = new Vector2(cols * 100f,  rows * 100f);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            canvasGroup.blocksRaycasts = false;
            startPosition = transform.position;
            startRotation = transform.rotation;
            transform.SetAsLastSibling();

            if (isPlacedOnGrid)
            {
                EventBus.Publish(new GameEvent.ItemRemovedEvent(itemData));
                isPlacedOnGrid = false;
            }
            
            RectTransform canvasRect = parentCanvas.transform as RectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 mouseLocalInCanvas))
            {
                localPointerOffset = (Vector2)rectTransform.localPosition - mouseLocalInCanvas;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform canvasRect = parentCanvas.transform as RectTransform;

            // Bám dính liên tục không độ trễ, không lệch hệ quy chiếu
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 mouseLocalInCanvas))
            {
                rectTransform.localPosition = mouseLocalInCanvas + localPointerOffset;
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

            int oldRows = currentShape.GetLength(0);// lay rows
            int oldColumns = currentShape.GetLength(1);//lay columns

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
            transform.position = initialTrayPosition;
            transform.rotation = Quaternion.identity;
            isPlacedOnGrid = false;
            if (itemData != null) currentShape = itemData.GetShapeMatrix();
            if (rectTransform != null && currentShape != null)
            {
                rectTransform.sizeDelta = new Vector2(currentShape.GetLength(1) * 100f, currentShape.GetLength(0) * 100f);
            }
            if (itemIcon != null) itemIcon.transform.rotation = Quaternion.identity;
            }
        }
}