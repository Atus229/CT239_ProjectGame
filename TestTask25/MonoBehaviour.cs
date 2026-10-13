using System;

namespace UnityEngine
{
    // Giả lập MonoBehaviour của Unity
    public class MonoBehaviour 
    { 
        public object gameObject { get; } = new object();
        public static void Destroy(object obj) { }
        public static void DontDestroyOnLoad(object obj) { }
    }

    public class ScriptableObject { }
    public class Sprite { }

    // Giả lập lớp Mathf của Unity
    public static class Mathf
    {
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
    }

    public struct Vector2Int
    {
        public int x, y;
        public Vector2Int(int x, int y) { this.x = x; this.y = y; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    // Attributes
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAssetMenuAttribute : Attribute
    {
        public string? menuName { get; set; }
        public string? fileName { get; set; }
        public int order { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class MinAttribute : Attribute
    {
        public MinAttribute(float min) { }
        public MinAttribute(int min) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SpaceAttribute : Attribute
    {
        public SpaceAttribute() { }
        public SpaceAttribute(float height) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string tooltip) { }
    }
}

// STUB DATA MODELS DÙNG CHO TEST
public class DayData : UnityEngine.ScriptableObject
{
    public int DayIndex { get; set; } = 1;
    public float DayTimeBudget { get; set; } = 220f;
}

public class JobData : UnityEngine.ScriptableObject
{
    public string JobId { get; set; } = "";
    public float TimeMin { get; set; } = 50f;
    public float TimeMax { get; set; } = 60f;
}

public struct DayHistoryRecord
{
    public int dayIndex;
    public int jobsCompleted;
    public int totalEarned;
    public float averageStars;
    public float usedTime;
}

// STUB MANAGERS DÙNG CHO TEST
public class SaveLoadManager
{
    public static SaveLoadManager? Instance { get; set; } = new SaveLoadManager();
    public void SaveDailyProgress(object record) { }
}

public class UIManager
{
    public static UIManager? Instance { get; set; } = new UIManager();
    public void ShowDaySummaryUI(object record) { }
}

public class ItemView { }