using System.Collections.Generic;
using UnityEngine;

public class AStarPathfindingSystem : MonoBehaviour
{
    private GridPathfinder _pathfinder;

    private void Awake()
    {
        _pathfinder = new GridPathfinder();
    }

    public List<GridCell> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        return _pathfinder.NavigatePath(start, goal);
    }
}