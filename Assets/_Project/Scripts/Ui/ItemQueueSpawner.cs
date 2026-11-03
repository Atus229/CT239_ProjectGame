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
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.JobSelectedEvent>(OnJobSelected);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromJobResult);
        }

        private void Start()
        {
            if (debugJobData != null && spawnedItems.Count == 0)
            {
                SpawnQueue(debugJobData);
            }
        }

        private void OnDestroy()
        {
            ClearQueue();
        }
        
        private void OnJobSelected(GameEvent.JobSelectedEvent evt)
        {
            if (evt.SelectedJob != null)
            {
                SpawnQueue(evt.SelectedJob);
            }
        }

        private void OnContinueFromJobResult(GameEvent.ContinueFromJobResultEvent evt)
        {
            ClearQueue();
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

                if (itemInstance.TryGetComponent<LayoutElement>(out var layout))
                {
                    layout.preferredWidth = itemRect.sizeDelta.x;
                    layout.preferredHeight = itemRect.sizeDelta.y;
                    layout.flexibleWidth = 0;
                    layout.flexibleHeight = 0;
                }

                itemInstance.SetupItem(itemData);
                spawnedItems.Add(itemInstance);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        }

        public void ClearQueue()
        {
            foreach (var item in spawnedItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }

            if (contentContainer != null)
            {
                for (int i = contentContainer.childCount - 1; i >= 0; i--)
                {
                    var child = contentContainer.GetChild(i);
                    if (child != null && child.gameObject != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            spawnedItems.Clear();
        }
    }
}
