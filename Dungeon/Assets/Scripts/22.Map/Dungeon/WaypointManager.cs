using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    private List<Vector2Int> _waypointPreset;

    private CellType[,] _grid;
    private int _gridWidth;
    private int _gridHeight;

    public void Init(MapData mapData)
    {
        _gridWidth = mapData.mapConfig.GridSize.x;
        _gridHeight = mapData.mapConfig.GridSize.y;

        _waypointPreset = new List<Vector2Int>();
    }

    private void GetWaypoint()
    {
        _waypointPreset.Clear();
        
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                if (_grid[i, j] == CellType.MainGate)
                {
                    _waypointPreset.Add(new Vector2Int(i,j));
                }
            }
        }
    }
}
