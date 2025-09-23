using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 생성 결과를 저장하는 데이터 클래스
/// </summary>
[System.Serializable]
public class MapData
{
    public CellType[,] grid;
    
    public List<RectInt> floorList;

    public MapGenerationConfig mapConfig;
    
    public int roomCount;
    
    public int corridorCount;
    
    public System.DateTime generationTime;
    
    public MapData(CellType[,] grid,List<RectInt> floorList, MapGenerationConfig mapConfig)
    {
        generationTime = System.DateTime.Now;
        this.mapConfig = mapConfig;
        this.floorList = floorList;
    }
    
    public CellType GetCellType(int x, int y)
    {
        if (grid == null || x < 0 || x >= mapConfig.GridSize.x || y < 0 || y >= mapConfig.GridSize.y)
            return CellType.Empty;
        
        return grid[x, y];
    }
    
    public void SetCellType(int x, int y, CellType cellType)
    {
        if (grid == null || x < 0 || x >= mapConfig.GridSize.x || y < 0 || y >= mapConfig.GridSize.y)
            return;
        
        grid[x, y] = cellType;
    }
    
    public void LogMapInfo()
    {
        Debug.Log($"맵 생성 완료 - 크기: {mapConfig.GridSize}, 방 개수: {roomCount}, 복도 개수: {corridorCount}, 생성 시간: {generationTime}");
    }
}
