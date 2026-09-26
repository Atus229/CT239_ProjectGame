using UnityEngine;

namespace _Project.Scripts.Core
{
    public struct ItemDroppedEvent
    {
        public string ItemName { get; private set; }
        public Vector3 DropPosition { get; private set; }

        public ItemDroppedEvent(string itemName, Vector3 dropPosition)
        {
            ItemName = itemName;
            DropPosition = dropPosition;
        }
    }
}