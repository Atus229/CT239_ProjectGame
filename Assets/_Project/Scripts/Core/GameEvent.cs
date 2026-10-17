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
            public bool[,] CurrentShape;

            public ItemDroppedEvent(ItemData itemPlaced, Vector3 dropPosition, ItemView sourceView, bool[,] currentShape)
            {
                this.ItemPlaced = itemPlaced;
                this.DropPosition = dropPosition;
                this.SourceView = sourceView;
                this.CurrentShape = currentShape;
            }
        }

        //Sự kiện bấm nút xác nhận giao hàng
        public struct JobSubmittedEvent
        {
            public JobData Job;
            public JobResult JobResult;

            public JobSubmittedEvent(JobData job, JobResult jobResult)
            {
                this.Job = job;
                this.JobResult = jobResult;
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

        public struct ItemRemovedEvent
        {
            public ItemData Item;

            public ItemRemovedEvent(ItemData item)
            {
                this.Item = item;
            }
        }

        public struct JobTimeUpdatedEvent
        {
            public float currentTime;
            public float MinTime;
            public float MaxTime;

            public JobTimeUpdatedEvent(float currentTime, float minTime, float maxTime)
            {
                this.currentTime = currentTime;
                this.MinTime = minTime;
                this.MaxTime = maxTime;
            }
        }

        public struct JobConfirmStateChangedEvent
        {
            public bool CanConfirm;

            public JobConfirmStateChangedEvent(bool canConfirm)
            {
                this.CanConfirm = canConfirm;
            }
        }

        public struct DayStartedEvent
        {
            public int dayIndex;
            public float dayTimeBudget;

            public DayStartedEvent(int dayIndex, float dayTimeBudget)
            {
                this.dayIndex = dayIndex;
                this.dayTimeBudget = dayTimeBudget;
            }
        }
        
        //Sự kiện cập nhật quỹ thời gian còn lại trong ngày
        public struct DayTimeUpdatedEvent
        {
            public float RemainingTime;
            public float Budget;

            public DayTimeUpdatedEvent(float remainingTime, float budget)
            {
                this.RemainingTime = remainingTime;
                this.Budget = budget;
            }
        }

        public struct DayEndedEvent
        {
            public DayHistoryRecord record;

            public DayEndedEvent(DayHistoryRecord record)
            {
                this.record = record;
            }
        }

        public struct JobSelectedEvent
        {
            public JobData job;

            public JobSelectedEvent(JobData job)
            {
                this.job = job;
            }
        }

        public struct JobCompletedEvent
        {
            public JobData job;
            public JobResult result;

            public JobCompletedEvent(JobData job, JobResult result)
            {
                this.job = job;
                this.result = result;
            }
        }
        
        //Su kien an tiep tuc tai man hinh result
        public struct ContinueFromJobResultEvent
        {
            //Dong vai tro la tin hieu -> rong
        }
        
        //Su kien khi nguoi choi an bat dau ngay moi
        public struct NextDayRequestedEvent
        {
            //Dong vai tro la tin hieu -> rong
        }
    }
}