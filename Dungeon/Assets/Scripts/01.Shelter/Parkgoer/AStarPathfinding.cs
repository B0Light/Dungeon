using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private AStarPathfindingGrid _pathfindingGrid;

    private void Awake()
    {
        _pathfindingGrid = new AStarPathfindingGrid();
    }

    public List<GridObject> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        return _pathfindingGrid.NavigatePath(start, goal);
    }
}