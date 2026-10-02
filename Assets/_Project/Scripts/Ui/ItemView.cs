using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Sử dụng nếu UI hiển thị tên dùng TextMeshPro
using _Project.Scripts.Core;
using _Project.Scripts.Data;

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

        private Vector3 startPosition;
        private CanvasGroup canvasGroup;
        

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

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
            canvasGroup.blocksRaycasts = false;
            startPosition = transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            Vector3 dropPos = transform.position;

            if (itemData != null)
            {
                // Bắn sự kiện ItemPlacedEvent gồm 3 tham số: ItemData, vị trí thả, và this (ItemView)
                EventBus.Publish(new GameEvent.ItemDroppedEvent(itemData, dropPos, this));
                Debug.Log($"[ItemView] Thả món: '{itemData.ItemName}' tại: {dropPos}");
            }

            // Trở về vị trí ban đầu trong hàng chờ
            ResetToStartPos();
        }

        private void ResetToStartPos()
        {
            transform.position = startPosition;
        }
    }
}