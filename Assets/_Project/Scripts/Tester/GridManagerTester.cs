<<<<<<< HEAD
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Scripts.Data;

namespace _Project.Scripts.Core
{
    public class GridManagerTester : MonoBehaviour
    {
        [Header("Tham chiếu Hệ thống")]
        public GridManager gridManager;
        public ItemData testItem;

        [Header("Giả lập Kéo - Thả")]
        [Tooltip("Kéo một GameObject ngẫu nhiên vào đây để giả lập món đồ đang cầm")]
        public Transform dragItem;

        // Ma trận lưu trạng thái xoay của vật phẩm test
        private bool[,] currentTestShape;

        private void Start()
        {
            if (testItem != null)
            {
                currentTestShape = testItem.GetShapeMatrix();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null || dragItem == null || gridManager == null || testItem == null) 
                return;

            if (currentTestShape == null)
            {
                currentTestShape = testItem.GetShapeMatrix();
            }

            // 1. Chuyển đổi tọa độ của GameObject mồi sang tọa độ Grid
            Vector2Int gridPos = gridManager.WorldToGridPosition(dragItem.position);

            // Bấm T: Xoay ma trận 90 độ (Rotate Test)
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                currentTestShape = RotateMatrixClockwise(currentTestShape);
                dragItem.Rotate(0f, 0f, -90f);
                Debug.Log($"[Test] Đã xoay vật phẩm '{testItem.ItemName}' sang kích thước mới: [{currentTestShape.GetLength(0)}, {currentTestShape.GetLength(1)}]");
            }

            // Bấm C: Kiểm tra tính hợp lệ (CanPlace)
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                bool canPlace = gridManager.CanPlace(gridPos, currentTestShape);
                Debug.Log($"[Test] Ô Grid: {gridPos} | Có thể đặt (CanPlace): {canPlace}");
            }

            // Bấm P: Đặt vật phẩm và Snap vào ô (PlaceItem)
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (gridManager.CanPlace(gridPos, currentTestShape))
                {
                    gridManager.PlaceItem(gridPos, testItem, currentTestShape);
                    // Hít vị trí về gốc ô lưới
                    dragItem.position = gridManager.GridToWorldPosition(gridPos);
                    Debug.Log($"[Test] ĐÃ ĐẶT '{testItem.ItemName}' tại {gridPos} thành công!");
                }
                else
                {
                    Debug.LogWarning($"[Test] KHÔNG THỂ ĐẶT tại {gridPos} (tràn biên hoặc trùng ô)!");
                }
            }

            // Bấm R: Gỡ vật phẩm (RemoveItem)
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                gridManager.RemoveItem(testItem);
                Debug.Log($"[Test] Đã gỡ bỏ toàn bộ ô của '{testItem.ItemName}' khỏi khoang xe.");
            }
        }

        // Hàm hỗ trợ xoay ma trận phục vụ test
        private bool[,] RotateMatrixClockwise(bool[,] original)
        {
            int oldRows = original.GetLength(0);
            int oldCols = original.GetLength(1);
            bool[,] rotated = new bool[oldCols, oldRows];

            for (int r = 0; r < oldRows; r++)
            {
                for (int c = 0; c < oldCols; c++)
                {
                    rotated[c, (oldRows - 1) - r] = original[r, c];
                }
            }
            return rotated;
        }
    }
=======
using _Project.Scripts.Data;

using UnityEngine;

using UnityEngine.InputSystem;



namespace _Project.Scripts.Core

{

    public class GridManagerTester : MonoBehaviour

    {

        public GridManager gridManager;

        public ItemData testItem;

        

        [Header("Thử nghiệm Kéo - Thả")]

        [Tooltip("Kéo một GameObject ngẫu nhiên vào đây để giả lập món đồ đang cầm")]

        public Transform dragItem; 



        private void Update()

        {

            // Đảm bảo bàn phím và cục mồi (dragItem) đang tồn tại

            if (Keyboard.current == null || dragItem == null) return;



            // 1. Chuyển đổi tọa độ thực tế của GameObject mồi thành tọa độ Grid

            Vector2Int gridPos = gridManager.WorldToGridPosition(dragItem.position);



            // Bấm C để Check

            if (Keyboard.current.cKey.wasPressedThisFrame)

            {

                bool canPlace = gridManager.CanPlace(gridPos, testItem);

                Debug.Log($"[Test] World: {dragItem.position} -> Ô Grid: {gridPos} | Can place: {canPlace}");

            }



            // Bấm P để Place và Snap (hít)

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (gridManager.CanPlace(gridPos, testItem))
                {
                    gridManager.PlaceItem(gridPos, testItem);
        
                    // Gán toạ độ căn giữa ô lưới
                    dragItem.position = gridManager.GridToWorldPosition(gridPos, snapToCenter: true);
        
                    Debug.Log($"[Test] Placed {testItem.ItemName} at {gridPos} and SNAPPED to CENTER!");
                }
                else
                {
                    Debug.LogWarning($"[Test] Cannot place at Grid: {gridPos}");
                }
            }


            // Bấm R để Remove

            if (Keyboard.current.rKey.wasPressedThisFrame)

            {

                gridManager.RemoveItem(gridPos, testItem);

                Debug.Log($"[Test] Removed from {gridPos}");

            }

        }

    }

>>>>>>> feat/task-19-jobeconomymanager
}