using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 생성 결과를 저장하는 데이터 클래스
/// </summary>
[System.Serializable]
public class MapData
{
    public FixedGridXZ<GridCell> grid;
    
    public List<RectInt> roomList;

    public MapGenerationConfig mapConfig;
    
    public int corridorCount;
    
    public System.DateTime generationTime;
    
    public MapData(FixedGridXZ<GridCell> grid, List<RectInt> roomList, MapGenerationConfig mapConfig, int corridorCount)
    {
        this.grid = grid;
        this.roomList = roomList;
        this.mapConfig = mapConfig;
        this.corridorCount = corridorCount;
        generationTime = System.DateTime.Now;
        
        LogMapInfo();
    }
    
    public CellType GetCellType(int x, int y)
    {
        if (grid == null || x < 0 || x >= mapConfig.GridSize.x || y < 0 || y >= mapConfig.GridSize.y)
            return CellType.Empty;
        
        return grid.GetGridObject(x,y).CellType;
    }
    
    public void LogMapInfo()
    {
        Debug.Log($"맵 생성 완료 - 크기: {mapConfig.GridSize}, 방 개수: {roomList.Count}, 복도 개수: {corridorCount}, 생성 시간: {generationTime}");
    }
}

/// <summary>
/// 맵 생성시 필요한 정보 
/// </summary>
public class MapGenerationConfig
{
    public Vector2Int GridSize { get; }
    public Vector3 CubeSize { get; }
    public int RoomSize { get; }
    public int Margin { get; }
    public TileMappingDataSO TileMappingDataSO { get; }
 
    public MapGenerationConfig(DungeonGenerateDataSO dungeonGenerateDataSo)
    {
        GridSize = dungeonGenerateDataSo.gridSize;
        CubeSize = dungeonGenerateDataSo.cubeSize;
        RoomSize = dungeonGenerateDataSo.roomSize;
        Margin = 3; 
        TileMappingDataSO = dungeonGenerateDataSo.tileMappingDataSO;
    }
}
