using System;
using System.Collections.Generic;
using _Project.Scripts.Data;
using _Project.Scripts.Managers;
using UnityEngine;
using _Project.Scripts.Ui;

namespace _Project.Scripts.Core
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private int columns = 4;
        [SerializeField] private int rows = 4;
        [SerializeField] private float cellSize = 100f;
        
        [Header("UI Grid Visual")]
        [SerializeField] private RectTransform truckBedGridRect; // Khung lưới TruckBedGrid trên Canvas

        private ItemData[,] grid;
        private readonly List<ItemView> placedItemViews = new List<ItemView>();

        public int Columns => columns;
        public int Rows => rows;
        public float CellSize => cellSize;
        public int TotalCellCount => columns * rows;
        
        private const int MAX_COLUMNS = 8; // Giới hạn tối đa chiều ngang thùng xe
        private const int MAX_ROWS = 8;    // Giới hạn tối đa chiều dọc thùng xe

        /// <summary>
        /// Số ô hiện đang chứa hàng hoá trên xe
        /// </summary>
        public int OccupiedCellCount
        {
            get
            {
                if (grid == null) return 0;
                int count = 0;
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        if (grid[x, y] != null) count++;
                    }
                }
                return count;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            grid = new ItemData[columns, rows];
        }
        
        /// <summary>
        /// Quy đổi từ vị trí thả chuột (Góc Dưới - Trái của ItemView) sang toạ độ ô lưới (X, Y).
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPosition, int shapeWidth = 1, int shapeHeight = 1)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);
            
            int x = Mathf.FloorToInt((localPos.x + (cellSize * 0.5f)) / cellSize);
            int y = Mathf.FloorToInt((localPos.y + (cellSize * 0.5f)) / cellSize);

            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Quy đổi từ toạ độ ô lưới (x, y) sang toạ độ thực tế trên Canvas.
        /// Trả về góc Dưới - Trái của ô lưới để khớp tuyệt đối với Pivot (0, 0) của ItemView.
        /// </summary>
        public Vector3 GridToWorldPosition(Vector2Int gridPosition, int shapeWidth = 1, int shapeHeight = 1)
        {
            float localCenterX = (gridPosition.x * cellSize);
            float localCenterY = (gridPosition.y * cellSize);

            return transform.TransformPoint(new Vector3(localCenterX, localCenterY, 0f));
        }

        public bool CanPlace(Vector2Int startPos, bool[,] shape)
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
            if (item == null || shape == null || !CanPlace(startPos, shape)) return;

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
            EventBus.Publish(new GameEvent.ItemPlacedEvent(item, startPos, dropPosition));

            // Phát âm thanh 'cộp' gỗ khi đặt đồ vào xe thành công
            AudioManager.Instance?.PlayPlaceSuccess();

            Debug.Log($"[GridManager] Đã xếp '{item.ItemName}' vào ô {startPos}. Số ô đã chiếm: {OccupiedCellCount}/{TotalCellCount}");
        }

        public void RemoveItem(ItemData item)
        {
            if (item == null || grid == null) return;

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (grid[x, y] == item)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }

        public void CleanGrid()
        {
            if (grid != null)
            {
                for (int x = 0; x < columns; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        grid[x, y] = null;
                    }
                }
            }

            // Xóa sạch toàn bộ GameObject ItemView đã xếp trên xe
            for (int i = placedItemViews.Count - 1; i >= 0; i--)
            {
                if (placedItemViews[i] != null && placedItemViews[i].gameObject != null)
                {
                    Destroy(placedItemViews[i].gameObject);
                }
            }
            placedItemViews.Clear();
            Debug.Log("<color=green>[GridManager]</color> Đã làm sạch toàn bộ thùng xe và xóa các vật phẩm cũ.");
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
            EventBus.Subscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromResult);
            EventBus.Subscribe<GameEvent.UpgradePurchasedEvent>(OnUpgradePurchased);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.ItemDroppedEvent>(OnItemDropped);
            EventBus.Unsubscribe<GameEvent.ItemRemovedEvent>(OnItemRemoved);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromResult);
            EventBus.Unsubscribe<GameEvent.UpgradePurchasedEvent>(OnUpgradePurchased);
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
                // Hút tâm ItemView vào chính giữa cụm ô đã chiếm
                e.SourceView.transform.position = GridToWorldPosition(gridCoord, shapeWidth, shapeHeight);
                e.SourceView.IsPlacedOnGrid = true;
                if (!placedItemViews.Contains(e.SourceView))
                {
                    placedItemViews.Add(e.SourceView);
                }
            }
            else
            {
                // Phát âm thanh 'tẹt' nhẹ khi đặt sai vị trí bay về khay
                AudioManager.Instance?.PlayPlaceFail();
                e.SourceView.ReturnToTrayPosition();
            }
        }
        
        private void OnItemRemoved(GameEvent.ItemRemovedEvent e)
        {
            if (e.Item != null)
            {
                RemoveItem(e.Item);
            }
            placedItemViews.RemoveAll(v => v == null || (v.Data == e.Item && !v.IsPlacedOnGrid));
        }

        private void OnContinueFromResult(GameEvent.ContinueFromJobResultEvent e)
        {
            CleanGrid();
        }
        
        [ContextMenu("TEST: Mở Rộng 4x4 lên 4x5")]
        public void TestExpand() => ExpandGrid(0, 1);

        /// <summary>
        /// Mở rộng kích thước khoang xe bằng cách cộng thêm số cột và số hàng.
        /// Giữ nguyên các hàng hóa hiện có trên lưới ô.
        /// API dành cho Leader gọi từ UpgradeManager hoặc Save/Load Manager.
        /// </summary>
        public void ExpandGrid(int extraCols, int extraRows)
        {
            if (extraCols <= 0 && extraRows <= 0) return;

            int oldCols = columns;
            int oldRows = rows;
            
            // Kẹp trần giá trị tối đa để bảo vệ bộ nhớ và bố cục UI
            int targetCols = Mathf.Clamp(columns + Mathf.Max(0, extraCols), 1, MAX_COLUMNS);
            int targetRows = Mathf.Clamp(rows + Mathf.Max(0, extraRows), 1, MAX_ROWS);
            
            // Nếu đã chạm mức tối đa thì không cần cấp phát lại mảng
            if (targetCols == oldCols && targetRows == oldRows)
            {
                Debug.LogWarning($"[GridManager] Đã đạt kích thước xe tối đa ({MAX_COLUMNS}x{MAX_ROWS}).");
                return;
            }
            
            columns = targetCols;
            rows = targetRows;
            ItemData[,] newGrid = new ItemData[columns, rows];
            // Sao chép dữ liệu từ ma trận cũ sang ma trận mới
            if (grid != null)
            {
                for (int x = 0; x < oldCols; x++)
                {
                    for (int y = 0; y < oldRows; y++)
                    {
                        newGrid[x, y] = grid[x, y];
                    }
                }
            }
            
            grid = newGrid;
            
            // Cập nhật lại kích thước khung lưới và sinh thêm các ô Slot xám trên UI
            if (truckBedGridRect != null)
            {
                // Mở rộng chiều cao khung lưới tương ứng số hàng mới
                truckBedGridRect.sizeDelta = new Vector2(columns * cellSize, rows * cellSize);

                int currentSlotCount = truckBedGridRect.childCount;
                int targetSlotCount = columns * rows;
                if (targetSlotCount > currentSlotCount && currentSlotCount > 0)
                {
                    // Lấy mẫu ô slot đầu tiên để nhân bản ra các ô mới
                    GameObject slotTemplate = truckBedGridRect.GetChild(0).gameObject;
                    for (int i = currentSlotCount; i < targetSlotCount; i++)
                    {
                        Instantiate(slotTemplate, truckBedGridRect);
                    }
                }
            }
            
            
            // Bắn tin cho UI biết để vẽ thêm các ô Slot mới!
            EventBus.Publish(new GameEvent.GridSizeChangedEvent(columns, rows));
            Debug.Log($"[GridManager] Đã mở rộng lưới xe: {oldCols}x{oldRows} -> {columns}x{rows}.");
        }
        
        private void OnUpgradePurchased(GameEvent.UpgradePurchasedEvent e)
        {
            if (e.UpgradeType == UpgradeData.TypeVehicleCapacity)
            {
                // Mở rộng thùng xe thêm 1 hàng
                ExpandGrid(0, 1);
            }
        }
    }
}
