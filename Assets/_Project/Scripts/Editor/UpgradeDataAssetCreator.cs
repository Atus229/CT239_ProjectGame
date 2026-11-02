#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using _Project.Scripts.Data;

namespace _Project.Scripts.Editor
{
    /// <summary>
    /// Công cụ Editor hỗ trợ tạo và cập nhật tự động các asset ScriptableObject UpgradeData chuẩn.
    /// Giúp Unity tự sinh cấu trúc binary/YAML chuẩn xác, tuyệt đối tránh lỗi bộ nhớ (Out of Memory).
    /// </summary>
    public static class UpgradeDataAssetCreator
    {
        [MenuItem("Tools/Upgrade System/Tạo dữ liệu UpgradeData chuẩn")]
        public static void GenerateDefaultUpgradeAssets()
        {
            string folderPath = "Assets/_Project/ScriptableObjects/UpgradeData";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            CreateOrUpdateAsset(
                1,
                "Mở rộng Quỹ thời gian ngày",
                UpgradeData.TypeDayTimeBudget,
                100,
                2.5f,
                3,
                30f,
                "Cộng thêm 30 giây vào Quỹ thời gian mỗi ngày (+30s/cấp).",
                "SO_Upgrade_1_DayTimeBudget.asset"
            );

            CreateOrUpdateAsset(
                2,
                "Mở rộng sức chứa khoang xe",
                UpgradeData.TypeVehicleCapacity,
                150,
                2.3333333f,
                2,
                1f,
                "Mở rộng kích thước khoang xe (+1 cột mỗi cấp).",
                "SO_Upgrade_2_VehicleCapacity.asset"
            );

            CreateOrUpdateAsset(
                3,
                "Nâng cấp hiệu suất xử lý",
                UpgradeData.TypeHandlingEfficiency,
                200,
                2.25f,
                2,
                0.10f,
                "Giảm 10% thời gian bốc xếp (Handling Time) của mọi hàng hóa (-10%/cấp).",
                "SO_Upgrade_3_HandlingEfficiency.asset"
            );

            CreateOrUpdateAsset(
                4,
                "Bảo hiểm trễ hẹn & Uy tín",
                UpgradeData.TypeLateGraceBuffer,
                300,
                2.0f,
                2,
                5f,
                "Nới rộng ngưỡng Tmax thêm 5 giây đệm (+5s/cấp) mà không bị trừ sao.",
                "SO_Upgrade_4_LateGraceBuffer.asset"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#4CAF50><b>[Upgrade System] Đã tự sinh thành công 4 file ScriptableObject UpgradeData chuẩn!</b></color>");
            EditorUtility.DisplayDialog("Thành Công", "Đã tạo/cập nhật thành công 4 gói UpgradeData trong Assets/_Project/ScriptableObjects/UpgradeData/", "OK");
        }

        private static void CreateOrUpdateAsset(int id, string name, string type, int baseCost, float costGrowth, int maxLevel, float effectValue, string description, string fileName)
        {
            string fullPath = $"Assets/_Project/ScriptableObjects/UpgradeData/{fileName}";
            UpgradeData asset = AssetDatabase.LoadAssetAtPath<UpgradeData>(fullPath);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UpgradeData>();
                asset.Init(id, name, type, baseCost, costGrowth, maxLevel, effectValue, description);
                AssetDatabase.CreateAsset(asset, fullPath);
            }
            else
            {
                asset.Init(id, name, type, baseCost, costGrowth, maxLevel, effectValue, description);
                EditorUtility.SetDirty(asset);
            }
        }
    }
}
#endif
