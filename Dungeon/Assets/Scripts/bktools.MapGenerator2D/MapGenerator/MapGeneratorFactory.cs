using System.Collections.Generic;
using UnityEngine;

public enum MapGeneratorType
{
    BSP,            // Binary Space Partitioning
    BSPFull,        // BSP Full (분할된 영역 전체를 방으로 사용)
    Isaac,          // Isaac 스타일 (BFS 방식)
    Delaunay        // Delaunay 삼각분할 + Kruskal
}

public class MapGeneratorFactory : MonoBehaviour
{
    [Header("맵 생성기 설정")]
    [SerializeField] private MapGeneratorType currentGeneratorType = MapGeneratorType.BSP;
    public MapGeneratorType CurrentGeneratorType => currentGeneratorType;
    
    [Header("기본 설정")]
    [SerializeField] private int seed = -1;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(64, 64);
    [SerializeField] private Vector3 cubeSize = new Vector3(2, 2, 2);
    [SerializeField] private int roomCount = 5;
    [SerializeField, Range(5,15)] private int standardRoomSize = 10;
    [SerializeField] private PathType pathType = PathType.LShaped;
    [SerializeField] private Transform slot; // 타일을 생성할 부모 Transform
    
    [Header("Tile Data Map")]
    [SerializeField] private TileMappingDataSO tileMappingDataSO;
    
    private BaseMapGenerator _currentGenerator;
    public BaseMapGenerator CurrentGenerator => _currentGenerator;
    
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        if (_currentGenerator == null)
        {
            SetupCurrentGenerator();
        }
        
        if (_currentGenerator == null)
        {
            Debug.LogWarning("맵 생성기를 설정할 수 없습니다. TileMappingDataSO가 할당되었는지 확인해주세요.");
            return;
        }
        
        Debug.Log($"{currentGeneratorType} 맵 생성기를 사용하여 맵을 생성합니다.");
        //ClearMap();
        _currentGenerator.GenerateMap();
    }
    
    private void SetupCurrentGenerator()
    {
        if (tileMappingDataSO == null)
        {
            Debug.LogError("TileMappingDataSO가 할당되지 않았습니다.");
            return;
        }
        
        // 새 생성기 생성
        switch (currentGeneratorType)
        {
            case MapGeneratorType.BSP:
                _currentGenerator = CreateBSPGenerator();
                break;
            case MapGeneratorType.BSPFull:
                _currentGenerator = CreateBSPFullGenerator();
                break;
            case MapGeneratorType.Isaac:
                _currentGenerator = CreateIsaacGenerator();
                break;
            case MapGeneratorType.Delaunay:
                _currentGenerator = CreateDelaunayGenerator();
                break;
            default:
                Debug.LogError($"알 수 없는 맵 생성기 타입: {currentGeneratorType}");
                return;
        }
    }
    
    private BSPDungeonMapGenerator CreateBSPGenerator()
    {
        var generator = new BSPDungeonMapGenerator(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            standardRoomSize -5, 
            standardRoomSize +5, 
            roomCount
        );
        
        generator.pathType = pathType;
        
        return generator;
    }
    
    private BSPDungeonMapGeneratorFull CreateBSPFullGenerator()
    {
        var generator = new BSPDungeonMapGeneratorFull(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            standardRoomSize-5, 
            standardRoomSize+5
        );
        
        generator.pathType = pathType;
        
        return generator;
    }
    
    private IsaacMapGenerator CreateIsaacGenerator()
    {
        var generator = new IsaacMapGenerator(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            roomCount, 
            3, 
            standardRoomSize, 
            standardRoomSize
        );
        
        // 경로 설정 적용
        generator.pathType = pathType;
        
        return generator;
    }
    
    private RandomMapGenerator CreateDelaunayGenerator()
    {
        var generator = new RandomMapGenerator(
            slot, 
            tileMappingDataSO, 
            gridSize, 
            cubeSize, 
            seed, 
            standardRoomSize, 
            standardRoomSize
        );
        
        // 경로 설정 적용
        generator.pathType = pathType;
        
        return generator;
    }
    
    public bool IsMapGenerated()
    {
        return _currentGenerator != null && _currentGenerator.IsMapGenerated;
    }
    
    
    public WaypointSystemData GetCurrentWaypointSystemData()
    {
        return _currentGenerator?.GetWaypointSystemData();
    }
    
    public Vector3 GetCubeSize()
    {
        return cubeSize;
    }
}
