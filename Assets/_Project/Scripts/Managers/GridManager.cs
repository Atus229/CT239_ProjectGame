using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;

        private ItemData[,] grid;

        private void Awake()
        {
            grid = new ItemData[columns, rows];
        }

        public bool CanPlace(Vector2Int startPos, ItemData item)
        {
            if (item == null) return false;

            bool[,] shape = item.GetShapeMatrix();

            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    if (shape[y, x])
                    {
                        int gridX = startPos.x + x;
                        int gridY = startPos.y + y;

                        if (gridX < 0 || gridX >= columns || gridY < 0 || gridY >= rows)
                            return false;

                        if (grid[gridX, gridY] != null)
                            return false;
                    }
                }
            }

            return true;
        }

        public void PlaceItem(Vector2Int startPos, ItemData item)
        {
            if (item == null || !CanPlace(startPos, item)) return;

            bool[,] shape = item.GetShapeMatrix();

            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    if (shape[y, x])
                    {
                        grid[startPos.x + x, startPos.y + y] = item;
                    }
                }
            }

            Vector3 dropPosition = new Vector3(startPos.x, startPos.y, 0f);
            EventBus.Publish(new GameEvent.ItemPlacedEvent(item, dropPosition));
        }

        public void RemoveItem(Vector2Int startPos, ItemData item)
        {
            if (item == null) return;

            bool[,] shape = item.GetShapeMatrix();

            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    if (shape[y, x])
                    {
                        int gridX = startPos.x + x;
                        int gridY = startPos.y + y;

                        if (gridX >= 0 && gridX < columns && gridY >= 0 && gridY < rows)
                        {
                            if (grid[gridX, gridY] == item)
                            {
                                grid[gridX, gridY] = null;
                            }
                        }
                    }
                }
            }
        }
    }
}