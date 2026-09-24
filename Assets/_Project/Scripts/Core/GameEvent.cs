using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class GameEvent
    {
        public struct ItemPlacedEvent
        {
            public ItemData ItemPlaced;
            public Vector3 DropPosition;

            public ItemPlacedEvent(ItemData itemPlaced, Vector3  dropPosition)
            {
                this.ItemPlaced = itemPlaced;
                this.DropPosition = dropPosition;
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
        
    }
}