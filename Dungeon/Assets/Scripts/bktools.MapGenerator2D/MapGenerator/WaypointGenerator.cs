using System.Collections.Generic;
using UnityEngine;

public class WaypointGenerator
{
    private MapData mapData;
    private Vector3 cubeSize;
    private WaypointSystemData waypointSystem;

    public WaypointGenerator(MapData mapData, Vector3 cubeSize)
    {
        this.mapData = mapData;
        this.cubeSize = cubeSize;
        this.waypointSystem = new WaypointSystemData();
        this.waypointSystem.gridSize = mapData.gridSize;
        this.waypointSystem.cubeSize = cubeSize;
    }

    public WaypointSystemData GenerateWaypointSystem()
    {
        if (mapData == null || !mapData.isGenerated)
        {
            Debug.LogError("맵 데이터가 유효하지 않습니다.");
            return null;
        }

        waypointSystem.waypoints.Clear();
        waypointSystem.patrolRoutes.Clear();

        GenerateRoomCenterWaypoints();
        GenerateCorridorIntersectionWaypoints();
        GenerateCorridorMidpointWaypoints();
        GenerateFloorWaypoints();
        DownsampleWaypoints();
        ConnectWaypointsOptimized();
        GeneratePatrolRoutes();

        Debug.Log($"웨이포인트 시스템 생성 완료: {waypointSystem.waypoints.Count}개 웨이포인트, {waypointSystem.patrolRoutes.Count}개 패트롤 경로");
        
        return waypointSystem;
    }

    private void GenerateRoomCenterWaypoints()
    {
        for (int i = 0; i < mapData.floorList.Count; i++)
        {
            RectInt room = mapData.floorList[i];
            Vector2Int centerGrid = new Vector2Int(
                room.x + (room.width - 1) / 2,
                room.y + (room.height - 1) / 2
            );
            Vector3 centerWorld = GridToWorldPosition(centerGrid);

            Waypoint roomWaypoint = new Waypoint(centerWorld, centerGrid, WaypointType.Room)
            {
                roomIndex = i,
                patrolRadius = Mathf.Min(room.width, room.height) * cubeSize.x * 0.3f
            };

            waypointSystem.AddWaypoint(roomWaypoint);
        }
    }

    private void GenerateCorridorIntersectionWaypoints()
    {
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();

        for (int x = 1; x < mapData.gridSize.x - 1; x++)
        {
            for (int y = 1; y < mapData.gridSize.y - 1; y++)
            {
                if (processedPositions.Contains(new Vector2Int(x, y)))
                    continue;

                if (IsPathCell(x, y) && IsIntersection(x, y))
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorldPosition(gridPos);

                    Waypoint intersectionWaypoint = new Waypoint(worldPos, gridPos, WaypointType.Intersection)
                    {
                        roomIndex = -1,
                        patrolRadius = cubeSize.x * 1.5f
                    };

                    waypointSystem.AddWaypoint(intersectionWaypoint);
                    processedPositions.Add(gridPos);
                }
            }
        }
    }

    private void GenerateCorridorMidpointWaypoints()
    {
        HashSet<Vector2Int> processedPositions = new HashSet<Vector2Int>();
        
        for (int y = 1; y < mapData.gridSize.y - 1; y++)
        {
            List<Vector2Int> horizontalPath = new List<Vector2Int>();
            for (int x = 1; x < mapData.gridSize.x - 1; x++)
            {
                if (IsPathCell(x, y) && !IsIntersection(x, y))
                {
                    horizontalPath.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (horizontalPath.Count >= 6)
                    {
                        CreateMidpointWaypoints(horizontalPath, processedPositions);
                    }
                    horizontalPath.Clear();
                }
            }
            if (horizontalPath.Count >= 6)
            {
                CreateMidpointWaypoints(horizontalPath, processedPositions);
            }
        }

        for (int x = 1; x < mapData.gridSize.x - 1; x++)
        {
            List<Vector2Int> verticalPath = new List<Vector2Int>();
            for (int y = 1; y < mapData.gridSize.y - 1; y++)
            {
                if (IsPathCell(x, y) && !IsIntersection(x, y) && !processedPositions.Contains(new Vector2Int(x, y)))
                {
                    verticalPath.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (verticalPath.Count >= 6)
                    {
                        CreateMidpointWaypoints(verticalPath, processedPositions);
                    }
                    verticalPath.Clear();
                }
            }
            if (verticalPath.Count >= 6)
            {
                CreateMidpointWaypoints(verticalPath, processedPositions);
            }
        }
    }

    private void CreateMidpointWaypoints(List<Vector2Int> pathPoints, HashSet<Vector2Int> processedPositions)
    {
        int interval = 8;
        for (int i = interval; i < pathPoints.Count; i += interval)
        {
            Vector2Int gridPos = pathPoints[i];
            if (!processedPositions.Contains(gridPos))
            {
                Vector3 worldPos = GridToWorldPosition(gridPos);
                Waypoint corridorWaypoint = new Waypoint(worldPos, gridPos, WaypointType.Corridor)
                {
                    roomIndex = -1,
                    patrolRadius = cubeSize.x
                };

                waypointSystem.AddWaypoint(corridorWaypoint);
                processedPositions.Add(gridPos);
            }
        }
    }

    private void GenerateFloorWaypoints()
    {
        int step = 6;
        for (int x = 1; x < mapData.gridSize.x - 1; x += step)
        {
            for (int y = 1; y < mapData.gridSize.y - 1; y += step)
            {
                if (IsWalkableCell(x, y))
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector3 worldPos = GridToWorldPosition(gridPos);

                    Waypoint floorWaypoint = new Waypoint(worldPos, gridPos, WaypointType.Room)
                    {
                        roomIndex = -1,
                        patrolRadius = cubeSize.x * 0.5f
                    };
                    waypointSystem.AddWaypoint(floorWaypoint);
                }
            }
        }
    }

    private void DownsampleWaypoints()
    {
        int targetCount = waypointSystem.waypoints.Count / 2;
        if (targetCount <= 0) return;

        List<Waypoint> reduced = new List<Waypoint>();
        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            if (i % 2 == 0)
                reduced.Add(waypointSystem.waypoints[i]);
        }
        waypointSystem.waypoints = reduced;
    }

    private void ConnectWaypointsOptimized()
    {
        Dictionary<Vector2Int, int> waypointLookup = new Dictionary<Vector2Int, int>();
        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            waypointLookup[waypointSystem.waypoints[i].gridPosition] = i;
        }

        Vector2Int[] neighborOffsets = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            Waypoint wp = waypointSystem.waypoints[i];
            foreach (var offset in neighborOffsets)
            {
                Vector2Int neighborPos = wp.gridPosition + offset;
                if (waypointLookup.TryGetValue(neighborPos, out int neighborIndex))
                {
                    if (HasClearPathFast(wp.gridPosition, waypointSystem.waypoints[neighborIndex].gridPosition))
                    {
                        waypointSystem.ConnectWaypoints(i, neighborIndex);
                    }
                }
            }
        }
    }
    
    private bool HasClearPathFast(Vector2Int start, Vector2Int end)
    {
        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (!IsWalkableCell(x0, y0)) return false;
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
        return true;
    }

    private void GeneratePatrolRoutes()
    {
        GenerateRoomPatrolRoutes();
        GenerateCorridorPatrolRoutes();
        GenerateGlobalPatrolRoute();
    }

    private void GenerateRoomPatrolRoutes()
    {
        for (int roomIndex = 0; roomIndex < mapData.floorList.Count; roomIndex++)
        {
            List<int> roomWaypointIndices = new List<int>();
            for (int i = 0; i < waypointSystem.waypoints.Count; i++)
            {
                if (waypointSystem.waypoints[i].roomIndex == roomIndex)
                {
                    roomWaypointIndices.Add(i);
                }
            }

            if (roomWaypointIndices.Count > 0)
            {
                PatrolRoute roomRoute = new PatrolRoute($"Room_{roomIndex}_Patrol", true)
                {
                    waitTime = 2f
                };
                roomRoute.waypointIndices = roomWaypointIndices;
                waypointSystem.AddPatrolRoute(roomRoute);
            }
        }
    }

    private void GenerateCorridorPatrolRoutes()
    {
        List<int> corridorWaypointIndices = new List<int>();
        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            Waypoint waypoint = waypointSystem.waypoints[i];
            if (waypoint.type == WaypointType.Corridor || waypoint.type == WaypointType.Intersection)
            {
                corridorWaypointIndices.Add(i);
            }
        }

        if (corridorWaypointIndices.Count > 1)
        {
            PatrolRoute corridorRoute = new PatrolRoute("Corridor_Patrol", false)
            {
                waitTime = 1f
            };
            corridorRoute.waypointIndices = corridorWaypointIndices;
            waypointSystem.AddPatrolRoute(corridorRoute);
        }
    }

    private void GenerateGlobalPatrolRoute()
    {
        if (waypointSystem.waypoints.Count < 2) return;

        List<int> globalRoute = new List<int>();
        for (int i = 0; i < waypointSystem.waypoints.Count; i++)
        {
            if (waypointSystem.waypoints[i].type == WaypointType.Room)
            {
                globalRoute.Add(i);
            }
        }

        if (globalRoute.Count > 1)
        {
            PatrolRoute globalPatrolRoute = new PatrolRoute("Global_Patrol", true)
            {
                waitTime = 3f
            };
            globalPatrolRoute.waypointIndices = globalRoute;
            waypointSystem.AddPatrolRoute(globalPatrolRoute);
        }
    }

    private bool IsIntersection(int x, int y)
    {
        if (!IsPathCell(x, y)) return false;

        int pathNeighbors = 0;
        if (IsWalkableCell(x - 1, y)) pathNeighbors++;
        if (IsWalkableCell(x + 1, y)) pathNeighbors++;
        if (IsWalkableCell(x, y - 1)) pathNeighbors++;
        if (IsWalkableCell(x, y + 1)) pathNeighbors++;

        return pathNeighbors >= 3;
    }

    private bool IsPathCell(int x, int y)
    {
        if (x < 0 || x >= mapData.gridSize.x || y < 0 || y >= mapData.gridSize.y)
            return false;

        CellType cellType = mapData.GetCellType(x, y);
        return cellType == CellType.Path || cellType == CellType.ExpandedPath;
    }

    private bool IsWalkableCell(int x, int y)
    {
        if (x < 0 || x >= mapData.gridSize.x || y < 0 || y >= mapData.gridSize.y)
            return false;

        CellType cellType = mapData.GetCellType(x, y);
        return cellType == CellType.Floor || 
               cellType == CellType.FloorCenter || 
               cellType == CellType.Path || 
               cellType == CellType.ExpandedPath;
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cubeSize.x, 0, gridPos.y * cubeSize.z);
    }

    public WaypointSystemData GetWaypointSystemData()
    {
        return waypointSystem;
    }
}
