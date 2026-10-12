
using System.Collections.Generic;
using UnityEngine;


namespace _Project.Scripts.Data
{
[CreateAssetMenu(fileName = "JobData", menuName = "Scriptable Objects/JobData")]
public class JobData : ScriptableObject
{
    [Header("Thong tin don hang")]
    [SerializeField] private string jobID;
    public string JobID 
    { 
        get { return jobID; } 
    }

    [SerializeField] private string customerName;
    public string CustomerName 
    { 
        get { return customerName; } 
    }

    [Header("Khung thoi gian muc tieu [Tmin, Tmax]")]
    [SerializeField] [Min(1f)] private float timeMin = 1f;
    public float TimeMin 
    { 
        get { return timeMin; } 
    }

    [SerializeField] [Min(1f)] private float timeMax = 1f;
    public float TimeMax 
    { 
        get { return timeMax; } 
    }

    [Header("Kho hang cho (Item Queue)")]
    [SerializeField] private List<ItemData> itemQueue = new List<ItemData>();
    public IReadOnlyList<ItemData> ItemQueue 
    { 
        get { return itemQueue; } 
    }

    [Header("Thuong mo rong")]
    [SerializeField] [Min(1f)] private float rewardMultiplier = 1f;
    public float RewardMultiplier 
    { 
        get { return rewardMultiplier; } 
    }

    private void OnValidate()
    {
        if (timeMax < timeMin)
        {
            timeMax = timeMin;
        }
    }
}
}
