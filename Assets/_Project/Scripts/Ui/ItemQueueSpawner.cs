using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Scripts.Core;
using _Project.Scripts.Data;

namespace _Project.Scripts.Ui
{
    public class ItemQueueSpawner : MonoBehaviour
    {
        [Tooltip("Prefab của từng món hàng (ItemView.prefab)")]
        [SerializeField] private ItemView itemViewPrefab;

        [Tooltip("Transform của Content bên trong ScrollRect Viewport")]
        [SerializeField] private RectTransform contentContainer;

        [Header("Slot / Card Sizing")]
        [Tooltip("Kích thước đồng nhất của mỗi thẻ hàng trong khay chờ (mặc định 120x120px)")]
        [SerializeField] private Vector2 queueCardSize = new Vector2(120f, 120f);

        [Header("Debugging / Testing")]
        [Tooltip("JobData mẫu để test thử hiển thị khi chạy Playmode")]
        [SerializeField] private JobData debugJobData;

        private readonly List<ItemView> spawnedItems = new List<ItemView>();
        public IReadOnlyList<ItemView> SpawnedItems => spawnedItems;

        private void Awake()
        {
            if (contentContainer == null)
            {
                ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
                if (scrollRect != null && scrollRect.content != null)
                {
                    contentContainer = scrollRect.content;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
        }

        private void Start()
        {
            if (debugJobData != null && spawnedItems.Count == 0)
            {
                SpawnQueue(debugJobData);
            }
        }

        private void OnJobSelected(GameEvent.JobSelectedEvent evt)
        {
            if (evt.SelectedJob != null)
            {
                SpawnQueue(evt.SelectedJob);
            }
        }

        /// <summary>
        /// Sinh các thẻ hàng hóa từ JobData
        /// </summary>
        public void SpawnQueue(JobData job)
        {
            if (job == null) return;
            SpawnQueue(job.ItemQueue);
        }

        /// <summary>
        /// Sinh các thẻ hàng hóa vào trong khay cuộn ScrollRect
        /// </summary>
        public void SpawnQueue(IReadOnlyList<ItemData> items)
        {
            ClearQueue();

            if (items == null || itemViewPrefab == null || contentContainer == null) return;

            foreach (var itemData in items)
            {
                if (itemData == null) continue;

                // Tạo Slot giữ chỗ với kích thước thẻ bài đồng nhất (queueCardSize)
                GameObject slotObj = new GameObject($"Slot_{itemData.ItemName}", typeof(RectTransform), typeof(LayoutElement));
                slotObj.transform.SetParent(contentContainer, false);

                RectTransform slotRect = slotObj.GetComponent<RectTransform>();
                slotRect.pivot = Vector2.zero;
                LayoutElement slotLayout = slotObj.GetComponent<LayoutElement>();

                slotRect.sizeDelta = queueCardSize;
                slotLayout.preferredWidth = queueCardSize.x;
                slotLayout.preferredHeight = queueCardSize.y;
                slotLayout.flexibleWidth = 0;
                slotLayout.flexibleHeight = 0;

                // Sinh ItemView làm con của Slot
                ItemView itemInstance = Instantiate(itemViewPrefab, slotObj.transform);
                RectTransform itemRect = itemInstance.GetComponent<RectTransform>();
                itemRect.pivot = Vector2.zero;
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.zero;
                itemRect.anchoredPosition = Vector2.zero;
                itemInstance.SetQueueCardSize(queueCardSize);
                itemInstance.SetupItem(itemData);
                spawnedItems.Add(itemInstance);
            }

            // Ép cập nhật lại Layout ngay lập tức để ScrollRect tính đúng kích thước Content
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        }

        /// <summary>
        /// Xóa sạch các thẻ hàng cũ khi đổi đơn hoặc reset
        /// </summary>
        public void ClearQueue()
        {
            for (int i = 0; i < spawnedItems.Count; i++)
            {
                if (spawnedItems[i] != null)
                {
                    Destroy(spawnedItems[i].gameObject);
                }
            }
            spawnedItems.Clear();

            if (contentContainer != null)
            {
                foreach (Transform child in contentContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        [ContextMenu("Test Spawn Debug Job")]
        public void TestSpawnDebugJob()
        {
            if (debugJobData != null)
            {
                SpawnQueue(debugJobData);
            }
            else
            {
                Debug.LogWarning("[ItemQueueSpawner] Chưa gán Debug Job Data để test!");
            }
        }
    }
}
