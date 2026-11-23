using System;
using System.Collections.Generic;
using System.IO;
using _Project.Scripts.Core;
using _Project.Scripts.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Project.Scripts.Editor
{
    public static class GameBalanceSetup
    {
        private const string ItemFolder = "Assets/_Project/ScriptableObjects/ItemData/Item";
        private const string JobFolder = "Assets/_Project/ScriptableObjects/JobData";
        private const string DayFolder = "Assets/_Project/ScriptableObjects/DayData";
        private const string PackingScenePath = "Assets/_Project/_Scenes/PackingScene.unity";

        private class JobConfig
        {
            public string RelativePath;
            public string JobID;
            public string CustomerName;
            public float TimeMin;
            public float TimeMax;
            public int RequiredMinGold;
            public string RequiredItemID;
            public string[] ItemIDs;
        }

        [MenuItem("Tools/Setup All Balanced Jobs and Days")]
        public static void SetupAllBalancedJobsAndDays()
        {
            try
            {
                // 1. Thu thập 20 ItemData từ thư mục
                var itemsByCode = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
                foreach (var guid in AssetDatabase.FindAssets("t:ItemData", new[] { ItemFolder }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                    if (item != null && !string.IsNullOrWhiteSpace(item.ItemID))
                    {
                        itemsByCode[item.ItemID] = item;
                    }
                }

                if (itemsByCode.Count < 20)
                {
                    Debug.LogWarning($"[GameBalanceSetup] Chỉ tìm thấy {itemsByCode.Count}/20 ItemData.");
                }

                // 2. Danh sách 21 cấu hình Job cho 7 ngày
                var jobConfigs = new List<JobConfig>
                {
                    // Ngày 1 (Xe 16 ô | 0$)
                    new JobConfig
                    {
                        RelativePath = "Day1/Job_Small.asset", JobID = "JOB_D1_S", CustomerName = "Sinh vien Nam",
                        TimeMin = 12f, TimeMax = 20f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_16", "ITEM_18", "ITEM_13", "ITEM_02", "ITEM_04", "ITEM_03" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day1/Job_Medium.asset", JobID = "JOB_D1_M", CustomerName = "Ban Han",
                        TimeMin = 20f, TimeMax = 30f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_01", "ITEM_16", "ITEM_06", "ITEM_17", "ITEM_18", "ITEM_02", "ITEM_04", "ITEM_13" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day1/Job_Large.asset", JobID = "JOB_D1_L", CustomerName = "Anh Tuan",
                        TimeMin = 30f, TimeMax = 42f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_01", "ITEM_11", "ITEM_06", "ITEM_16", "ITEM_17", "ITEM_02", "ITEM_02", "ITEM_18", "ITEM_13", "ITEM_04" }
                    },

                    // Ngày 2 (Xe 16 ô | 0$)
                    new JobConfig
                    {
                        RelativePath = "Day2/Job_Small.asset", JobID = "JOB_D2_S", CustomerName = "Co Lan",
                        TimeMin = 18f, TimeMax = 28f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_15", "ITEM_16", "ITEM_06", "ITEM_17", "ITEM_13", "ITEM_18", "ITEM_04", "ITEM_02" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day2/Job_Medium.asset", JobID = "JOB_D2_M", CustomerName = "Cap doi moi cuoi",
                        TimeMin = 25f, TimeMax = 36f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_01", "ITEM_11", "ITEM_06", "ITEM_16", "ITEM_09", "ITEM_15", "ITEM_17", "ITEM_18", "ITEM_13", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day2/Job_Large.asset", JobID = "JOB_D2_L", CustomerName = "Chi Mai",
                        TimeMin = 32f, TimeMax = 44f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_07", "ITEM_01", "ITEM_11", "ITEM_06", "ITEM_09", "ITEM_16", "ITEM_15", "ITEM_17", "ITEM_03", "ITEM_18", "ITEM_02" }
                    },

                    // Ngày 3 (Xe 20 ô)
                    new JobConfig
                    {
                        RelativePath = "Day3/Job_Small.asset", JobID = "JOB_D3_S", CustomerName = "Tiem giat ui",
                        TimeMin = 24f, TimeMax = 35f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_15", "ITEM_15", "ITEM_16", "ITEM_06", "ITEM_18", "ITEM_13", "ITEM_04", "ITEM_02", "ITEM_03" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day3/Job_Medium.asset", JobID = "JOB_D3_M", CustomerName = "Anh Hung (1PN)",
                        TimeMin = 34f, TimeMax = 46f, RequiredMinGold = 80, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_07", "ITEM_11", "ITEM_01", "ITEM_06", "ITEM_09", "ITEM_16", "ITEM_15", "ITEM_17", "ITEM_18", "ITEM_02", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day3/Job_VIP.asset", JobID = "JOB_D3_VIP", CustomerName = "Hoang Gia (VIP)",
                        TimeMin = 42f, TimeMax = 56f, RequiredMinGold = 150, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_05", "ITEM_07", "ITEM_11", "ITEM_01", "ITEM_09", "ITEM_16", "ITEM_15", "ITEM_17", "ITEM_06", "ITEM_18", "ITEM_13", "ITEM_03", "ITEM_02" }
                    },

                    // Ngày 4 (Xe 20 ô)
                    new JobConfig
                    {
                        RelativePath = "Day4/Job_Small.asset", JobID = "JOB_D4_S", CustomerName = "Tiem banh ngot",
                        TimeMin = 26f, TimeMax = 38f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_01", "ITEM_15", "ITEM_13", "ITEM_13", "ITEM_17", "ITEM_18", "ITEM_02", "ITEM_04", "ITEM_20" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day4/Job_Medium.asset", JobID = "JOB_D4_M", CustomerName = "Van phong IT",
                        TimeMin = 36f, TimeMax = 48f, RequiredMinGold = 100, RequiredItemID = "ITEM_09",
                        ItemIDs = new[] { "ITEM_09", "ITEM_05", "ITEM_11", "ITEM_01", "ITEM_15", "ITEM_17", "ITEM_18", "ITEM_18", "ITEM_02", "ITEM_02", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day4/Job_Large.asset", JobID = "JOB_D4_L", CustomerName = "Nha pho chi Hoa",
                        TimeMin = 44f, TimeMax = 58f, RequiredMinGold = 160, RequiredItemID = "ITEM_05",
                        ItemIDs = new[] { "ITEM_05", "ITEM_07", "ITEM_11", "ITEM_01", "ITEM_09", "ITEM_16", "ITEM_06", "ITEM_17", "ITEM_15", "ITEM_18", "ITEM_13", "ITEM_03" }
                    },

                    // Ngày 5 (Xe 25 ô)
                    new JobConfig
                    {
                        RelativePath = "Day5/Job_Small.asset", JobID = "JOB_D5_S", CustomerName = "Tiem tra hoa",
                        TimeMin = 30f, TimeMax = 42f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_17", "ITEM_20", "ITEM_20", "ITEM_03", "ITEM_15", "ITEM_18", "ITEM_02", "ITEM_02", "ITEM_13", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day5/Job_Medium.asset", JobID = "JOB_D5_M", CustomerName = "Can ho cao cap",
                        TimeMin = 46f, TimeMax = 60f, RequiredMinGold = 130, RequiredItemID = "ITEM_19",
                        ItemIDs = new[] { "ITEM_19", "ITEM_07", "ITEM_05", "ITEM_01", "ITEM_09", "ITEM_16", "ITEM_06", "ITEM_17", "ITEM_15", "ITEM_18", "ITEM_20", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day5/Job_VIP.asset", JobID = "JOB_D5_VIP", CustomerName = "Biet thu VIP",
                        TimeMin = 52f, TimeMax = 68f, RequiredMinGold = 220, RequiredItemID = "ITEM_14",
                        ItemIDs = new[] { "ITEM_14", "ITEM_19", "ITEM_07", "ITEM_05", "ITEM_11", "ITEM_01", "ITEM_09", "ITEM_06", "ITEM_17", "ITEM_15", "ITEM_20", "ITEM_03", "ITEM_18", "ITEM_02" }
                    },

                    // Ngày 6 (Xe 25 ô)
                    new JobConfig
                    {
                        RelativePath = "Day6/Job_Small.asset", JobID = "JOB_D6_S", CustomerName = "Phong tam Master",
                        TimeMin = 32f, TimeMax = 45f, RequiredMinGold = 0, RequiredItemID = null,
                        ItemIDs = new[] { "ITEM_08", "ITEM_15", "ITEM_20", "ITEM_18", "ITEM_13", "ITEM_02", "ITEM_04", "ITEM_03", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day6/Job_Medium.asset", JobID = "JOB_D6_M", CustomerName = "Penthouse Khach",
                        TimeMin = 48f, TimeMax = 64f, RequiredMinGold = 160, RequiredItemID = "ITEM_10",
                        ItemIDs = new[] { "ITEM_10", "ITEM_09", "ITEM_17", "ITEM_20", "ITEM_19", "ITEM_07", "ITEM_05", "ITEM_01", "ITEM_15", "ITEM_18", "ITEM_02", "ITEM_02", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day6/Job_Large.asset", JobID = "JOB_D6_L", CustomerName = "Penthouse Ngu",
                        TimeMin = 55f, TimeMax = 72f, RequiredMinGold = 240, RequiredItemID = "ITEM_12",
                        ItemIDs = new[] { "ITEM_12", "ITEM_08", "ITEM_09", "ITEM_07", "ITEM_10", "ITEM_01", "ITEM_11", "ITEM_17", "ITEM_15", "ITEM_20", "ITEM_18", "ITEM_03", "ITEM_02", "ITEM_04" }
                    },

                    // Ngày 7 (Xe 30 ô MAX)
                    new JobConfig
                    {
                        RelativePath = "Day7/Job_Small.asset", JobID = "JOB_D7_S", CustomerName = "Can ho ven song",
                        TimeMin = 40f, TimeMax = 54f, RequiredMinGold = 0, RequiredItemID = "ITEM_17",
                        ItemIDs = new[] { "ITEM_17", "ITEM_08", "ITEM_01", "ITEM_09", "ITEM_16", "ITEM_06", "ITEM_15", "ITEM_13", "ITEM_20", "ITEM_18", "ITEM_02", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day7/Job_Medium.asset", JobID = "JOB_D7_M", CustomerName = "Biet thu nghi duong",
                        TimeMin = 56f, TimeMax = 72f, RequiredMinGold = 220, RequiredItemID = "ITEM_19",
                        ItemIDs = new[] { "ITEM_19", "ITEM_10", "ITEM_08", "ITEM_09", "ITEM_07", "ITEM_11", "ITEM_01", "ITEM_05", "ITEM_17", "ITEM_15", "ITEM_20", "ITEM_03", "ITEM_18", "ITEM_02", "ITEM_04" }
                    },
                    new JobConfig
                    {
                        RelativePath = "Day7/Job_SuperVIP.asset", JobID = "JOB_D7_SVIP", CustomerName = "Tru so Tap doan (SIEU VIP)",
                        TimeMin = 68f, TimeMax = 85f, RequiredMinGold = 350, RequiredItemID = "ITEM_14",
                        ItemIDs = new[] { "ITEM_14", "ITEM_12", "ITEM_10", "ITEM_19", "ITEM_07", "ITEM_05", "ITEM_11", "ITEM_08", "ITEM_09", "ITEM_01", "ITEM_17", "ITEM_06", "ITEM_16", "ITEM_15", "ITEM_20", "ITEM_03", "ITEM_18", "ITEM_04" }
                    }
                };

                // 3. Khởi tạo / Cập nhật từng JobData
                var jobsByDay = new Dictionary<int, List<JobData>>();
                for (int d = 1; d <= 7; d++) jobsByDay[d] = new List<JobData>();

                foreach (var cfg in jobConfigs)
                {
                    string path = $"{JobFolder}/{cfg.RelativePath}";
                    string dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    var job = AssetDatabase.LoadAssetAtPath<JobData>(path);
                    bool isNew = job == null;
                    if (isNew)
                    {
                        job = ScriptableObject.CreateInstance<JobData>();
                    }

                    var serialized = new SerializedObject(job);
                    serialized.FindProperty("jobID").stringValue = cfg.JobID;
                    serialized.FindProperty("customerName").stringValue = cfg.CustomerName;
                    serialized.FindProperty("timeMin").floatValue = cfg.TimeMin;
                    serialized.FindProperty("timeMax").floatValue = cfg.TimeMax;
                    serialized.FindProperty("rewardMultiplier").floatValue = 1f;
                    serialized.FindProperty("requiredMinGold").intValue = cfg.RequiredMinGold;

                    ItemData reqItem = null;
                    if (!string.IsNullOrEmpty(cfg.RequiredItemID) && itemsByCode.TryGetValue(cfg.RequiredItemID, out reqItem))
                    {
                        serialized.FindProperty("requiredItem").objectReferenceValue = reqItem;
                    }
                    else
                    {
                        serialized.FindProperty("requiredItem").objectReferenceValue = null;
                    }

                    var queueProp = serialized.FindProperty("itemQueue");
                    queueProp.ClearArray();
                    for (int i = 0; i < cfg.ItemIDs.Length; i++)
                    {
                        string id = cfg.ItemIDs[i];
                        if (itemsByCode.TryGetValue(id, out var itm))
                        {
                            queueProp.InsertArrayElementAtIndex(queueProp.arraySize);
                            queueProp.GetArrayElementAtIndex(queueProp.arraySize - 1).objectReferenceValue = itm;
                        }
                    }

                    serialized.ApplyModifiedPropertiesWithoutUndo();

                    if (isNew)
                    {
                        AssetDatabase.CreateAsset(job, path);
                    }

                    // Phân loại vào ngày
                    int dayIdx = int.Parse(cfg.RelativePath.Substring(3, 1));
                    jobsByDay[dayIdx].Add(job);
                }

                // 4. Tạo hoặc Cập nhật 7 DayData
                float[] budgets = { 90f, 105f, 130f, 145f, 165f, 180f, 210f };
                var allScheduledDays = new List<DayData>();

                for (int d = 1; d <= 7; d++)
                {
                    string dayPath = $"{DayFolder}/Day{d}Data.asset";
                    var day = AssetDatabase.LoadAssetAtPath<DayData>(dayPath);
                    bool isNewDay = day == null;
                    if (isNewDay)
                    {
                        day = ScriptableObject.CreateInstance<DayData>();
                    }

                    var serDay = new SerializedObject(day);
                    serDay.FindProperty("dayIndex").intValue = d;
                    serDay.FindProperty("dayTimeBudget").floatValue = budgets[d - 1];

                    var dailyJobsProp = serDay.FindProperty("dailyJobs");
                    dailyJobsProp.ClearArray();
                    foreach (var j in jobsByDay[d])
                    {
                        dailyJobsProp.InsertArrayElementAtIndex(dailyJobsProp.arraySize);
                        dailyJobsProp.GetArrayElementAtIndex(dailyJobsProp.arraySize - 1).objectReferenceValue = j;
                    }

                    serDay.ApplyModifiedPropertiesWithoutUndo();

                    if (isNewDay)
                    {
                        AssetDatabase.CreateAsset(day, dayPath);
                    }

                    allScheduledDays.Add(day);
                }

                // 5. Cập nhật DayManager trong PackingScene
                try
                {
                    var activeScene = EditorSceneManager.GetActiveScene();
                    bool openedScene = false;
                    if (activeScene.path != PackingScenePath)
                    {
                        EditorSceneManager.OpenScene(PackingScenePath, OpenSceneMode.Single);
                        openedScene = true;
                    }

                    var dayMgr = UnityEngine.Object.FindFirstObjectByType<DayManager>();
                    if (dayMgr != null)
                    {
                        var serMgr = new SerializedObject(dayMgr);
                        serMgr.FindProperty("currentDayData").objectReferenceValue = allScheduledDays[0];
                        var schedProp = serMgr.FindProperty("scheduledDays");
                        schedProp.ClearArray();
                        foreach (var d in allScheduledDays)
                        {
                            schedProp.InsertArrayElementAtIndex(schedProp.arraySize);
                            schedProp.GetArrayElementAtIndex(schedProp.arraySize - 1).objectReferenceValue = d;
                        }
                        serMgr.ApplyModifiedProperties();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                        Debug.Log("[GameBalanceSetup] Đã gán thành công 7 DayData vào DayManager.scheduledDays trong PackingScene!");
                    }
                }
                catch (Exception sceneEx)
                {
                    Debug.LogWarning($"[GameBalanceSetup] Không thể tự động cập nhật Scene: {sceneEx.Message}");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"<color=#00FF00><b>[GameBalanceSetup] THÀNH CÔNG! Đã cập nhật xong 21 JobData và 7 DayData!</b></color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameBalanceSetup] Lỗi: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
