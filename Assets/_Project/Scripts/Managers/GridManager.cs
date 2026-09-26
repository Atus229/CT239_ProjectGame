using _Project.Scripts.Data;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;
        [SerializeField] private float cellSize = 1f;

        private ItemData[,] grid;

        private void Awake()
        {
            grid = new ItemData[columns, rows];
        }
        
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - transform.position;
            int x = Mathf.FloorToInt(localPosition.x / cellSize);
            int y = Mathf.FloorToInt(localPosition.y / cellSize);
            
            return new Vector2Int(x, y);
        }
        
        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            float worldX = gridPosition.x * cellSize;
            float worldY = gridPosition.y * cellSize;
            
            return transform.position + new Vector3(worldX, worldY, 0f);
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

            Vector3 dropPosition = GridToWorldPosition(startPos);
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

        private void OnDrawGizmos()
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (Application.isPlaying && grid != null && grid[x, y] != null)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }

                    Vector3 cellCenter = GridToWorldPosition(new Vector2Int(x, y)) + new Vector3(cellSize / 2f, cellSize / 2f, 0f);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize * 0.95f, cellSize * 0.95f, 0.1f));
                }
            }
        }
    }
}