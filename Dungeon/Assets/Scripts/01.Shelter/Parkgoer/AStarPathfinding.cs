using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private GridXZ<GridObject> _grid;
    private GridObject _goalNode;
    public List<GridObject> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        _grid = GridBuildingSystem.Instance.GetGrid();

        List<GridObject> path = FindPath(start, goal);
        
        return path;
    }

    private List<GridObject> FindPath(Vector2Int start, Vector2Int goal)
    {
        GridObject startNode = _grid.GetGridObject(start.x, start.y);
        _goalNode = _grid.GetGridObject(goal.x, goal.y);
        
        List<GridObject> openList = new List<GridObject>();
        HashSet<GridObject> closedList = new HashSet<GridObject>();
        
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            GridObject currentNode = openList.OrderBy(node => node.FCost).First();

            if (currentNode == _goalNode)
            {
                return RetracePath(startNode, _goalNode);
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (GridObject neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                float tentativeGCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetDistance(neighbor, _goalNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null; // 경로를 찾지 못한 경우
    }

    private List<GridObject> RetracePath(GridObject startNode, GridObject goalNode)
    {
        List<GridObject> path = new List<GridObject>();
        GridObject currentNode = goalNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }

    private IEnumerable<GridObject> GetNeighbors(GridObject node)
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
            var neighborGrid = _grid.GetGridObject(neighborPos.x, neighborPos.y);
            
            if (neighborGrid?.GetTileType() == TileType.Road) 
            {
                neighbors.Add(_grid.GetGridObject(neighborPos.x, neighborPos.y));
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
                        neighbors.Add(_grid.GetGridObject(neighborPos.x, neighborPos.y));
                    }
                }
            }
        }
        return neighbors;
    }
    
    private static BuildObjData.Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return BuildObjData.Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return BuildObjData.Dir.Right;
        if (direction == new Vector2Int(0, -1)) return BuildObjData.Dir.Up;
        if (direction == new Vector2Int(1, 0)) return BuildObjData.Dir.Left;
        return BuildObjData.Dir.Down;
    }

    private float GetDistance(GridObject a, GridObject b)
    {
        int distX = Mathf.Abs(a.GetEntrancePosition().x - b.GetEntrancePosition().x);
        int distY = Mathf.Abs(a.GetEntrancePosition().y - b.GetEntrancePosition().y);
        return distX + distY; // 맨해튼 거리
    }
    
}