using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private AStarPathfindingGridObject _pathfindingGridObject;

    private void Awake()
    {
        _pathfindingGridObject = new AStarPathfindingGridObject();
    }

    public List<GridObject> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        return _pathfindingGridObject.NavigatePath(start, goal);
    }
}