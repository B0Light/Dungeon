using System.Collections.Generic;
using UnityEngine;

public class AStarPathfindingSystem : MonoBehaviour
{
    private AStarPathfinding _pathfinding;

    private void Awake()
    {
        _pathfinding = new AStarPathfinding();
    }

    public List<GridCell> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        return _pathfinding.NavigatePath(start, goal);
    }
}