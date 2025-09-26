using System.Collections.Generic;
using System.Linq;
using bkTools;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    private List<Vector2Int> _waypointPreset;
    private List<Vector2Int> _patrolPath;
    private Dictionary<Vector2Int, List<Vector2Int>> _waypointConnections;

    private MapData _mapData;

    public void Init(MapData mapData)
    {
        _mapData = mapData;
        
        _waypointPreset = new List<Vector2Int>();

        GetWaypoint();
    }

    private void GetWaypoint()
    {
        _waypointPreset.Clear();
        
        for (int i = 0; i < _mapData.mapConfig.GridSize.x; i++)
        {
            for (int j = 0; j < _mapData.mapConfig.GridSize.y; j++)
            {
                if (_mapData.GetCellType(i,j) == CellType.MainGate)
                {
                    _waypointPreset.Add(new Vector2Int(i,j));
                }
            }
        }
    }
}
