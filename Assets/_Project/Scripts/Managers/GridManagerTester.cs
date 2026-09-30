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

}