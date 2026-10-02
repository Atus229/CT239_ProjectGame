using _Project.Scripts.Data;
using UnityEngine;
using _Project.Scripts.Ui;

namespace _Project.Scripts.Core
{
    public class GameEvent
    {
        public struct ItemDroppedEvent
        {
            public ItemData ItemPlaced;
            public Vector3 DropPosition;
            public ItemView SourceView;

            public ItemDroppedEvent(ItemData itemPlaced, Vector3 dropPosition, ItemView sourceView)
            {
                this.ItemPlaced = itemPlaced;
                this.DropPosition = dropPosition;
                this.SourceView = sourceView;
            }
        }

        //Sự kiện bấm nút xác nhận giao hàng
        public struct JobSubmittedEvent
        {
            public JobData Job;
            public float TotalHandlingTime;

            public JobSubmittedEvent(JobData job, float totalHandlingTime)
            {
                this.Job = job;
                this.TotalHandlingTime = totalHandlingTime;
            }
        }

        public struct ItemPlacedEvent
        {
            public ItemData Item;
            public Vector2Int GridPosition;
            public Vector3 WorldPosition;
            
            public ItemPlacedEvent(ItemData item, Vector2Int gridPosition, Vector3 worldPosition)
            {
                Item = item;
                GridPosition = gridPosition;
                WorldPosition = worldPosition;
            }
        }
            
        
    }
}