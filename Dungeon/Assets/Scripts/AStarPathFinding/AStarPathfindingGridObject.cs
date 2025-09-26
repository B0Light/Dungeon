using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathfindingGridObject : AStarPathfindingBase<GridObject>
{
    private GridXZ<GridObject> _grid;
    private GridObject _goalNode;

    public List<GridObject> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        _grid = GridBuildingSystem.Instance.GetGrid();

        GridObject startNode = _grid.GetGridObject(start.x, start.y);
        _goalNode = _grid.GetGridObject(goal.x, goal.y);
        
        // GCost와 HCost 초기화
        foreach (GridObject obj in _grid.GetAllGridObjects())
        {
            obj.GCost = float.MaxValue;
            obj.HCost = float.MaxValue;
            obj.Parent = null;
        }
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, _goalNode);

        List<GridObject> path = FindPath(startNode, _goalNode);
        
        return path;
    }

    protected override IEnumerable<GridObject> GetNeighbors(GridObject node)
    {
        List<GridObject> neighbors = new List<GridObject>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 상
            new Vector2Int(0, -1),  // 하
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(1, 0)    // 우
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = node.GetEntrancePosition() + dir;
            GridObject neighborGrid = _grid.GetGridObject(neighborPos.x, neighborPos.y);
            
            if (neighborGrid?.GetTileType() == TileType.Road) 
            {
                neighbors.Add(neighborGrid);
            }
            
            if ((neighborGrid?.GetTileType() == TileType.Headquarter || neighborGrid?.GetTileType() == TileType.Attraction || neighborGrid?.GetTileType() == TileType.MajorFacility)
                && neighborGrid == _goalNode)
            {
                Vector2Int attractionOrigin = neighborGrid.GetEntrancePosition();
                if (attractionOrigin == neighborPos)
                {
                    BuildObjData.Dir objectDirection = neighborGrid.GetDirection(); // 방향 가져오기

                    if (objectDirection == ConvertToConnectDirection(dir))
                    {
                        neighbors.Add(neighborGrid);
                    }
                }
            }
        }
        return neighbors;
    }
    
    protected override float GetDistance(GridObject a, GridObject b)
    {
        int distX = Mathf.Abs(a.GetEntrancePosition().x - b.GetEntrancePosition().x);
        int distY = Mathf.Abs(a.GetEntrancePosition().y - b.GetEntrancePosition().y);
        return distX + distY; // 맨해튼 거리
    }

    private static BuildObjData.Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return BuildObjData.Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return BuildObjData.Dir.Right;
        if (direction == new Vector2Int(0, -1)) return BuildObjData.Dir.Up;
        if (direction == new Vector2Int(1, 0)) return BuildObjData.Dir.Left;
        return BuildObjData.Dir.Down;
    }
}
