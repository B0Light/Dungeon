using System;
using UnityEngine;
using System.Collections.Generic;

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager Instance;
    
    private Dictionary<int, int> _killLog = new Dictionary<int, int>();
    
    [Header("Waypoint System")]
    [SerializeField]
    private MapGeneratorFactory mapGeneratorFactory;
    
    private void Awake()
    {
        Instance = this;
        
        if (mapGeneratorFactory == null)
            mapGeneratorFactory = FindFirstObjectByType<MapGeneratorFactory>();
    }
    
    public void AddKillLog(int itemId)
    {
        if (_killLog.TryGetValue(itemId, out int currentCount))
        {
            _killLog[itemId] = currentCount + 1;
        }
        else
        {
            _killLog[itemId] = 1;
        }
    }

    public Dictionary<int, int> GetKillLog() => _killLog;
}
