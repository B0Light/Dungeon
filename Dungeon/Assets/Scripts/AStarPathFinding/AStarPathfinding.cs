using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : AStarPathfindingBase<GridCell>
{
    private GridXZ<GridCell> _grid;
    private GridCell _goalNode;

    public List<GridCell> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        _grid = GridBuildingSystem.Instance.GetGrid();

        GridCell startNode = _grid.GetGridObject(start.x, start.y);
        _goalNode = _grid.GetGridObject(goal.x, goal.y);
        
        // GCost와 HCost 초기화
        foreach (GridCell obj in _grid.GetAllGridObjects())
        {
            obj.GCost = float.MaxValue;
            obj.HCost = float.MaxValue;
            obj.Parent = null;
        }
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, _goalNode);

        List<GridCell> path = FindPath(startNode, _goalNode);
        
        return path;
    }

    protected override IEnumerable<GridCell> GetNeighbors(GridCell node)
    {
        List<GridCell> neighbors = new List<GridCell>();

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
            GridCell neighborGrid = _grid.GetGridObject(neighborPos.x, neighborPos.y);
            
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
    
    protected override float GetDistance(GridCell a, GridCell b)
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
