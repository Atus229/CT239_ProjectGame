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

        [Header("UI Grid Visual")] [SerializeField]
        private RectTransform truckBedGridRect; // Khung lưới TruckBedGrid trên Canvas

        [Header("Truck Bed Centering & Auto-Fit")]
        [Tooltip("Toạ độ tâm của khoang thùng xe tải (Khuyến nghị X: 0, Y: 25 để vào chính giữa sàn gỗ)")]
        [SerializeField] private Vector2 truckBedCenter = new Vector2(0f, 25f);

        [Tooltip("Kích thước tối đa của khoang thùng xe tải (để tự co nhỏ nếu quá nhiều ô)")]
        [SerializeField] private Vector2 maxTruckBedSize = new Vector2(500f, 520f);

        [Tooltip("Tự động co nhỏ kích thước ô nếu số hàng/cột vượt quá khoang thùng xe")]
        [SerializeField] private bool autoFitInsideTruck = true;

        [Header("Grid Slot Sprites (Ưu tiên dùng Sprite pixel nếu có)")]
        [Tooltip("Ảnh ô trống (Nâu gỗ)")] [SerializeField]
        private Sprite spriteEmpty;

        [Tooltip("Ảnh ô đã chứa đồ (Nâu đậm / bóng râm)")] [SerializeField]
        private Sprite spriteOccupied;

        [Tooltip("Ảnh ô xem trước rê đồ vào vừa (Xanh lá)")] [SerializeField]
        private Sprite spriteValidHover;

        [Tooltip("Ảnh ô xem trước rê đồ vào cấn/thiếu (Đỏ cảnh báo)")] [SerializeField]
        private Sprite spriteInvalidHover;

        [Header("Grid Slot Colors (Dùng khi chưa gán Sprite)")] [Tooltip("Màu cơ bản khi ô trống (Nâu nhạt sàn gỗ)")] [SerializeField]
        private Color colorEmpty = new Color32(188, 143, 98, 255); // #BC8F62 (Nâu cơ bản)

        [Tooltip("Màu khi đã đặt đồ vào thành công (Nâu sẫm đậm hơn)")] [SerializeField]
        private Color colorOccupied = new Color32(105, 68, 40, 255); // #694428 (Nâu đậm)

        [Tooltip("Màu xem trước khi rê đồ vào đủ chỗ (Xanh lá)")] [SerializeField]
        private Color colorValidHover = new Color32(92, 195, 92, 230); // #5CC35C (Xanh)

        [Tooltip("Màu xem trước khi rê đồ vào bị cấn/tràn (Đỏ cảnh báo)")] [SerializeField]
        private Color colorInvalidHover = new Color32(230, 75, 75, 230); // #E64B4B (Đỏ)

        // Dan sách lưu toạ độ các ô đang được tô màu Preview để hoàn lại màu khi dời chuột
        private readonly List<Vector2Int> currentPreviewCells = new List<Vector2Int>();

        private ItemData[,] grid;
        private readonly List<ItemView> placedItemViews = new List<ItemView>();

        public int Columns => columns;
        public int Rows => rows;
        public float CellSize => cellSize;
        public int TotalCellCount => columns * rows;

        private const int MAX_COLUMNS = 8; // Giới hạn tối đa chiều ngang thùng xe
        private const int MAX_ROWS = 8; // Giới hạn tối đa chiều dọc thùng xe

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

        private int baseColumns = 4;
        private int baseRows = 4;

        internal void RestoreVehicleCapacity(int level)
        {
            if (level <= 0) return;
            int extraCols = (level + 1) / 2;
            int extraRows = level / 2;

            int targetCols = Mathf.Clamp(baseColumns + extraCols, 1, MAX_COLUMNS);
            int targetRows = Mathf.Clamp(baseRows + extraRows, 1, MAX_ROWS);

            int addCols = Mathf.Max(0, targetCols - columns);
            int addRows = Mathf.Max(0, targetRows - rows);
            if (addCols > 0 || addRows > 0)
            {
                ExpandGrid(addCols, addRows);
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

            if (colorEmpty.a == 0) colorEmpty = new Color32(188, 143, 98, 255);
            if (colorOccupied.a == 0) colorOccupied = new Color32(105, 68, 40, 255);
            if (colorValidHover.a == 0) colorValidHover = new Color32(92, 195, 92, 230);
            if (colorInvalidHover.a == 0) colorInvalidHover = new Color32(230, 75, 75, 230);

            // Tự động sửa tâm về vị trí chuẩn nếu còn lưu giá trị cũ (-71)
            if (truckBedCenter.y < -50f)
            {
                truckBedCenter = new Vector2(0f, 25f);
            }

            if (maxTruckBedSize.x <= 0f)
            {
                maxTruckBedSize = new Vector2(500f, 520f);
            }

            if (truckBedGridRect == null)
            {
                truckBedGridRect = transform.Find("TruckBedGrid") as RectTransform;
            }

            baseColumns = columns;
            baseRows = rows;
            grid = new ItemData[columns, rows];
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (truckBedGridRect != null && !Application.isPlaying)
            {
                UpdateTruckBedLayout();
            }
        }
#endif

        private void Start()
        {
            UpdateTruckBedLayout(); // Căn chính xác tâm lưới vào giữa thùng xe
            RefreshAllSlotColors(); // Đổi toàn bộ ô sang Sprite/Màu cơ bản ngay khi vào game
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
                        RefreshAllSlotColors(); // Đổi các ô đã chiếm sang Nâu đậm hơn
                    }
                }
            }

            Vector3 dropPosition = GridToWorldPosition(startPos);
            EventBus.Publish(new GameEvent.ItemPlacedEvent(item, startPos, dropPosition));

            // Phát âm thanh 'cộp' gỗ khi đặt đồ vào xe thành công
            AudioManager.Instance?.PlayPlaceSuccess();

            Debug.Log(
                $"[GridManager] Đã xếp '{item.ItemName}' vào ô {startPos}. Số ô đã chiếm: {OccupiedCellCount}/{TotalCellCount}");
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
                        RefreshAllSlotColors(); // Đổi các ô vừa nhấc ra về lại Nâu cơ bản
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
                RefreshAllSlotColors();
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
            ClearPlacementPreview();
            Debug.Log("<color=green>[GridManager]</color> Đã làm sạch toàn bộ thùng xe và xóa các vật phẩm cũ.");
        }

        /// <summary>
        /// Kiểm tra xem một loại món đồ cụ thể có đang được xếp trên lưới thùng xe hay không.
        /// </summary>
        public bool HasItemOnGrid(ItemData itemData)
        {
            if (itemData == null) return true;
            for (int i = 0; i < placedItemViews.Count; i++)
            {
                if (placedItemViews[i] != null && placedItemViews[i].IsPlacedOnGrid)
                {
                    ItemData data = placedItemViews[i].Data;
                    if (data != null && (data == itemData || (!string.IsNullOrEmpty(data.ItemID) && data.ItemID == itemData.ItemID)))
                    {
                        return true;
                    }
                }
            }
            return false;
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
            EventBus.Subscribe<GameEvent.DayEndedEvent>(OnDayEnded);
            EventBus.Subscribe<GameEvent.DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEvent.ItemDroppedEvent>(OnItemDropped);
            EventBus.Unsubscribe<GameEvent.ItemRemovedEvent>(OnItemRemoved);
            EventBus.Unsubscribe<GameEvent.ContinueFromJobResultEvent>(OnContinueFromResult);
            EventBus.Unsubscribe<GameEvent.UpgradePurchasedEvent>(OnUpgradePurchased);
            EventBus.Unsubscribe<GameEvent.DayEndedEvent>(OnDayEnded);
            EventBus.Unsubscribe<GameEvent.DayStartedEvent>(OnDayStarted);
        }

        private void OnDayEnded(GameEvent.DayEndedEvent e)
        {
            CleanGrid();
        }

        private void OnDayStarted(GameEvent.DayStartedEvent e)
        {
            CleanGrid();
        }

        private void OnItemDropped(GameEvent.ItemDroppedEvent e)
        {
            if (e.ItemPlaced == null || e.SourceView == null) return;

            bool[,] shape = e.CurrentShape;
            int shapeHeight = shape.GetLength(0); // Số dòng (trục Y)
            int shapeWidth = shape.GetLength(1); // Số cột (trục X)

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

        [ContextMenu("Căn Giữa Lưới")]
        public void UpdateTruckBedLayout()
        {
            if (truckBedGridRect == null)
            {
                truckBedGridRect = transform.Find("TruckBedGrid") as RectTransform;
                if (truckBedGridRect == null) return;
            }

            // 1. Tự co kích thước ô nếu lưới quá lớn để không bao giờ bị tràn thùng xe
            if (autoFitInsideTruck)
            {
                float fitWidth = maxTruckBedSize.x / Mathf.Max(1, columns);
                float fitHeight = maxTruckBedSize.y / Mathf.Max(1, rows);
                cellSize = Mathf.Min(110f, Mathf.Min(fitWidth, fitHeight));
            }

            // 2. Cập nhật kích thước khung lưới
            truckBedGridRect.sizeDelta = new Vector2(columns * cellSize, rows * cellSize);

            // 3. Cập nhật GridLayoutGroup
            var gridLayout = truckBedGridRect.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = columns;
                float spacingX = gridLayout.spacing.x;
                float spacingY = gridLayout.spacing.y;
                gridLayout.cellSize = new Vector2(cellSize - spacingX, cellSize - spacingY);
            }

            // 4. Căn giữa: Dời góc dưới-trái (Origin) để tâm lưới luôn trùng với tâm thùng xe
            float posX = truckBedCenter.x - (columns * cellSize * 0.5f);
            float posY = truckBedCenter.y - (rows * cellSize * 0.5f);
            transform.localPosition = new Vector3(posX, posY, 0f);
        }

        [ContextMenu("TEST: Thêm 1 Hàng (+1 Row)")]
        public void TestAddRow() => ExpandGrid(0, 1);

        [ContextMenu("TEST: Thêm 1 Cột (+1 Col)")]
        public void TestAddColumn() => ExpandGrid(1, 0);

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

            UpdateTruckBedLayout();
            RefreshAllSlotColors();

            // Bắn tin cho UI biết để vẽ thêm các ô Slot mới!
            EventBus.Publish(new GameEvent.GridSizeChangedEvent(columns, rows));
            Debug.Log($"[GridManager] Đã mở rộng lưới xe: {oldCols}x{oldRows} -> {columns}x{rows}.");
        }

        private void OnUpgradePurchased(GameEvent.UpgradePurchasedEvent e)
        {
            if (e.UpgradeType == UpgradeData.TypeVehicleCapacity)
            {
                // Cấp lẻ (Lv1, Lv3): Thêm 1 cột ngang (ExpandGrid(1, 0))
                // Cấp chẵn (Lv2): Thêm 1 hàng dọc (ExpandGrid(0, 1))
                if (e.NewLevel % 2 != 0)
                {
                    ExpandGrid(1, 0); // Lv1: 4x4 -> 5x4 | Lv3: 5x5 -> 6x5
                }
                else
                {
                    ExpandGrid(0, 1); // Lv2: 5x4 -> 5x5
                }
            }
        }

        /// <summary>
        /// Lấy component Image của ô slot tại toạ độ (x, y) trên giao diện xe tải
        /// </summary>
        public UnityEngine.UI.Image GetSlotImage(int x, int y)
        {
            if (truckBedGridRect == null) return null;
            int index = y * columns + x;
            if (index >= 0 && index < truckBedGridRect.childCount)
            {
                return truckBedGridRect.GetChild(index).GetComponent<UnityEngine.UI.Image>();
            }

            return null;
        }

        /// <summary>
        /// Làm mới toàn bộ màu hoặc sprite của các ô lưới theo trạng thái thực tế: Có đồ -> Nâu đậm, Trống -> Nâu cơ bản
        /// </summary>
        public void RefreshAllSlotColors()
        {
            if (truckBedGridRect == null || grid == null) return;

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    var img = GetSlotImage(x, y);
                    if (img != null)
                    {
                        bool isOccupied = (grid[x, y] != null);
                        if (spriteEmpty != null || spriteOccupied != null)
                        {
                            img.sprite = isOccupied ? spriteOccupied : spriteEmpty;
                            img.color = Color.white;
                        }
                        else
                        {
                            // Nếu ô đã có đồ -> Nâu đậm, nếu trống -> Nâu cơ bản
                            img.color = isOccupied ? colorOccupied : colorEmpty;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gọi khi người chơi đang rê món đồ qua xe tải:
        /// Đủ chỗ -> Tô Xanh (hoặc sprite Xanh) | Thiếu/Cấn -> Tô Đỏ (hoặc sprite Đỏ)
        /// </summary>
        public void ShowPlacementPreview(Vector3 worldPosition, bool[,] shape)
        {
            if (shape == null || truckBedGridRect == null) return;

            // 1. Trả các ô preview cũ về màu thực tế trước đó
            ClearPlacementPreview();

            // 2. Tính toạ độ ô lưới góc dưới-trái món đồ
            Vector2Int startPos = WorldToGridPosition(worldPosition);
            bool canPlace = CanPlace(startPos, shape);
            Color previewColor = canPlace ? colorValidHover : colorInvalidHover;

            int shapeHeight = shape.GetLength(0);
            int shapeWidth = shape.GetLength(1);

            // 3. Quét qua các ô mà món đồ sẽ chiếm
            for (int r = 0; r < shapeHeight; r++)
            {
                for (int c = 0; c < shapeWidth; c++)
                {
                    if (shape[r, c])
                    {
                        int gx = startPos.x + c;
                        int gy = startPos.y + r;

                        // Nếu ô nằm trong phạm vi thùng xe
                        if (gx >= 0 && gx < columns && gy >= 0 && gy < rows)
                        {
                            var slotImg = GetSlotImage(gx, gy);
                            if (slotImg != null)
                            {
                                if (canPlace && spriteValidHover != null)
                                {
                                    slotImg.sprite = spriteValidHover;
                                    slotImg.color = Color.white;
                                }
                                else if (!canPlace && spriteInvalidHover != null)
                                {
                                    slotImg.sprite = spriteInvalidHover;
                                    slotImg.color = Color.white;
                                }
                                else
                                {
                                    slotImg.color = previewColor;
                                }

                                currentPreviewCells.Add(new Vector2Int(gx, gy));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Xoá màu xem trước (Xanh/Đỏ), trả các ô về đúng màu/sprite Nâu của chúng
        /// </summary>
        public void ClearPlacementPreview()
        {
            if (currentPreviewCells.Count == 0) return;

            foreach (var cell in currentPreviewCells)
            {
                if (cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows)
                {
                    var img = GetSlotImage(cell.x, cell.y);
                    if (img != null && grid != null)
                    {
                        bool isOccupied = (grid[cell.x, cell.y] != null);
                        if (spriteEmpty != null || spriteOccupied != null)
                        {
                            img.sprite = isOccupied ? spriteOccupied : spriteEmpty;
                            img.color = Color.white;
                        }
                        else
                        {
                            img.color = isOccupied ? colorOccupied : colorEmpty;
                        }
                    }
                }
            }

            currentPreviewCells.Clear();
        }
    }
}