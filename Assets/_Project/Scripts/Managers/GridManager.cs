using System;
using _Project.Scripts.Data;
using UnityEngine;
using _Project.Scripts.Ui;

namespace _Project.Scripts.Core
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;
        [SerializeField] private float cellSize = 100f;

        private ItemData[,] grid;

        private void Awake()
        {
            grid = new ItemData[columns, rows];
        }
        
        public Vector2Int WorldToGridPosition(Vector3 worldPosition, int shapeWidth = 1, int shapeHeight = 1)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            // 1. Tìm vị trí mép dưới-trái thực tế bằng cách lấy tâm trừ đi nửa kích thước
            float bottomLeftX = localPos.x - (shapeWidth * cellSize / 2f);
            float bottomLeftY = localPos.y - (shapeHeight * cellSize / 2f);

            // 2. Chia cho cellSize và lấy phần sàn (FloorToInt) với hệ số dung sai 0.5 ô
            int x = Mathf.FloorToInt((bottomLeftX + (cellSize * 0.5f)) / cellSize);
            int y = Mathf.FloorToInt((bottomLeftY + (cellSize * 0.5f)) / cellSize);

            return new Vector2Int(x, y);
        }
        
        public Vector3 GridToWorldPosition(Vector2Int gridPosition, int shapeWidth = 1, int shapeHeight = 1)
        {
            // Tọa độ tâm = Tọa độ gốc ô + Nửa chiều rộng/cao của khối hình
            float localCenterX = (gridPosition.x * cellSize) + (shapeWidth * cellSize / 2f);
            float localCenterY = (gridPosition.y * cellSize) + (shapeHeight * cellSize / 2f);

            return transform.TransformPoint(new Vector3(localCenterX, localCenterY, 0f));
        }

        public bool CanPlace(Vector2Int startPos,  bool[,] shape)
        {
            if (shape == null) return false;

            int shapeHeight = shape.GetLength(0);
            int shapeWidth = shape.GetLength(1);

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (shape[r, c])
                    {
                        int gridX = startPos.x + c;
                        int gridY = startPos.y + r;

                        if (gridX < 0 || gridX >= columns || gridY < 0 || gridY >= rows)
                            return false;

                        if (grid[gridX, gridY] != null)
                            return false;
                    }
                }
            }

            return true;
        }

        public void PlaceItem(Vector2Int startPos, ItemData item, bool[,] shape)
        {
            if (item == null ||shape == null|| !CanPlace(startPos, shape)) return;

            int shapeHeight = shape.GetLength(0);
            int shapeWidth = shape.GetLength(1);

            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (shape[r, c])
                    {
                        grid[startPos.x + c, startPos.y + r] = item;
                    }
                }
            }

            Vector3 dropPosition = GridToWorldPosition(startPos);
            EventBus.Publish(new GameEvent.ItemPlacedEvent(item, startPos ,dropPosition));
            Debug.Log($"[GridManager] Đã xếp '{item.ItemName}' vào ô {startPos}");
        }

        public void RemoveItem(ItemData item)
        {
            if (item == null) return;

            bool hasRemoved = false;

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (grid[x, y] == item)
                    {
                        grid[x, y] = null;
                        hasRemoved = true;
                    }
                }
            }

            if (hasRemoved)
            {
                EventBus.Publish(new GameEvent.ItemRemovedEvent(item));
                Debug.Log($"[GridManager] Đã gỡ bỏ món '{item.ItemName}' khỏi khoang xe.");
            }
        }

        private void OnDrawGizmos()
        {
            if (cellSize <= 0f) cellSize = 100f;
            
            Gizmos.matrix = transform.localToWorldMatrix;
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

                    Vector3 cellCenter = new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0f);
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize * 0.95f, cellSize * 0.95f, 0.1f));
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEvent.ItemDroppedEvent>(OnItemDropped);
            EventBus.Subscribe<GameEvent.ItemRemovedEvent>(OnItemRemoved);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.ItemDroppedEvent>(OnItemDropped);
            EventBus.Subscribe<GameEvent.ItemRemovedEvent>(OnItemRemoved);
        }

        private void OnItemDropped(GameEvent.ItemDroppedEvent e)
        {
            if (e.ItemPlaced == null || e.SourceView == null) return;

            bool[,] shape = e.CurrentShape;
            int shapeHeight = shape.GetLength(0); // Số dòng (trục Y)
            int shapeWidth = shape.GetLength(1);  // Số cột (trục X)

            // Quy đổi từ tâm thả chuột về góc Dưới - Trái của ma trận
            Vector2Int gridCoord = WorldToGridPosition(e.DropPosition, shapeWidth, shapeHeight);

            if (CanPlace(gridCoord, shape))
            {
                PlaceItem(gridCoord, e.ItemPlaced, shape);
                // Hít tâm ItemView vào chính giữa cụm ô đã chiếm
                e.SourceView.transform.position = GridToWorldPosition(gridCoord, shapeWidth, shapeHeight);
                e.SourceView.isPlaceOnGrid = true;
            }
            else
            {
                e.SourceView.ReturnToTrayPosition();
            }
        }
        
        private void OnItemRemoved(GameEvent.ItemRemovedEvent e)
        {
            if (e.Item != null)
            {
                RemoveItem(e.Item);
            }
        }

        
    }
}