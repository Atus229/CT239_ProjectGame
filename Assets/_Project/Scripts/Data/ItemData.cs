using UnityEngine;

namespace _Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "New_ItemData", menuName = "Scriptable Objects/ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("Dinh danh va hien thi")] [SerializeField]
        private string itemID;

        public string ItemID
        {
            get { return itemID; }
        }

        [SerializeField] private string itemName;

        public string ItemName
        {
            get { return itemName; }
        }

        [SerializeField] private Sprite itemIcon;

        public Sprite ItemIcon
        {
            get { return itemIcon; }
        }

        [Header("Khong gian luoi (Grid)")] [SerializeField] [Min(1)]
        private int width = 1;

        public int Width
        {
            get { return width; }
        }

        [SerializeField] [Min(1)] private int height = 1;

        public int Height
        {
            get { return height; }
        }

        [Header("Cau hinh hinh dang chi tiet")]
        // Mảng 1D lưu hình dạng của 1 item -> bằng cách true false
        //Do Mảng 2D không serializefield được
        [SerializeField]
        private bool[] shapeSerialized;

        [Header("Kinh te va thoi gian")] [SerializeField] [Min(1)]
        private int value = 1;

        public int Value
        {
            get { return value; }
        }

        [SerializeField] [Min(0.1f)] private float handlingTime = 1f;

        public float HandlingTime
        {
            get { return handlingTime; }
        }

        [Header("Phat trien them")] [SerializeField]
        private bool isFragile;

        public bool IsFragile
        {
            get { return isFragile; }
        }

        //Chuyển mảng 1D thành ma trận bool[,] theo chuẩn nhóm đã thống nhất.
        // Gốc [0,0] là góc dưới - bên trái.
        public bool[,] GetShapeMatrix()
        {
            bool[,] matrix = new bool[height, width];

            //y - hang, x - cot
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    matrix[y, x] = shapeSerialized[index];
                }
            }

            return matrix;
        }
    }
}