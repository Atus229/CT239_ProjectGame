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

        public void SpawnQueue(JobData job)
        {
            if (job == null) return;
            SpawnQueue(job.ItemQueue);
        }

        public void SpawnQueue(IReadOnlyList<ItemData> items)
        {
            ClearQueue();

            if (items == null || itemViewPrefab == null || contentContainer == null) return;

            foreach (var itemData in items)
            {
                if (itemData == null) continue;

                ItemView itemInstance = Instantiate(itemViewPrefab, contentContainer);
                RectTransform itemRect = itemInstance.GetComponent<RectTransform>();
                itemRect.pivot = Vector2.zero;

                LayoutElement layout = itemInstance.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = itemInstance.gameObject.AddComponent<LayoutElement>();
                }
                layout.preferredWidth = itemRect.sizeDelta.x;
                layout.preferredHeight = itemRect.sizeDelta.y;
                layout.flexibleWidth = 0;
                layout.flexibleHeight = 0;

                itemInstance.SetupItem(itemData);
                spawnedItems.Add(itemInstance);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        }

        public void ClearQueue()
        {
            for (int i = 0; i < spawnedItems.Count; i++)
            {
                if (spawnedItems[i] != null)
                {
                    // Nếu item đang nằm trên xe tải, gỡ khỏi GridManager trước khi hủy GameObject
                    if (spawnedItems[i].IsPlacedOnGrid && GridManager.Instance != null)
                    {
                        GridManager.Instance.RemoveItem(spawnedItems[i].Data);
                    }
                    Destroy(spawnedItems[i].gameObject);
                }
            }
            spawnedItems.Clear();

            if (contentContainer != null)
            {
                for (int i = contentContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(contentContainer.GetChild(i).gameObject);
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
