using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private DungeonDataSO dungeonDataSo;
    [SerializeField] private Transform slot;
    private int _seed;
    private BaseMapGenerator _currentGenerator;
    
    public void GenerateMap()
    {
        SetupCurrentGenerator();
        if (_currentGenerator == null)
        {
            Debug.LogWarning("맵 생성기를 설정할 수 없습니다. TileMappingDataSO가 할당되었는지 확인해주세요.");
            return;
        }

        // 시드값이 0이면 새로운 시드 생성, 아니면 고정 시드 사용
        if (dungeonDataSo.seed == 0)
        {
            _seed = System.DateTime.Now.Second;
        }
        _currentGenerator.GenerateMap(_seed);
    }
    
    private void SetupCurrentGenerator()
    {
        if (dungeonDataSo == null)
        {
            Debug.LogError("TileMappingDataSO가 할당되지 않았습니다.");
            return;
        }
        
        // 새 생성기 생성
        switch (dungeonDataSo.generatorType)
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
                Debug.LogError($"알 수 없는 맵 생성기 타입: {dungeonDataSo.generatorType}");
                return;
        }
    }
    
    private BSPDungeonMapGenerator CreateBSPGenerator()
    {
        var generator = new BSPDungeonMapGenerator(
            slot, 
            dungeonDataSo.tileMappingDataSO, 
            dungeonDataSo.gridSize, 
            dungeonDataSo.cubeSize, 
            dungeonDataSo.roomSize -5, 
            dungeonDataSo.roomSize +5 
        );
        
        generator.pathType = dungeonDataSo.pathType;
        
        return generator;
    }
    
    private BSPDungeonMapGeneratorFull CreateBSPFullGenerator()
    {
        var generator = new BSPDungeonMapGeneratorFull(
            slot, 
            dungeonDataSo.tileMappingDataSO, 
            dungeonDataSo.gridSize, 
            dungeonDataSo.cubeSize, 
            dungeonDataSo.roomSize 
        );
        
        generator.pathType = dungeonDataSo.pathType;
        
        return generator;
    }
    
    private IsaacMapGenerator CreateIsaacGenerator()
    {
        var generator = new IsaacMapGenerator(
            slot, 
            dungeonDataSo.tileMappingDataSO, 
            dungeonDataSo.gridSize, 
            dungeonDataSo.cubeSize,  
            3, 
            dungeonDataSo.roomSize, 
            dungeonDataSo.roomSize
        );
        
        // 경로 설정 적용
        generator.pathType = dungeonDataSo.pathType;
        
        return generator;
    }
    
    private DelaunayMapGenerator CreateDelaunayGenerator()
    {
        var generator = new DelaunayMapGenerator(
            slot, 
            dungeonDataSo.tileMappingDataSO, 
            dungeonDataSo.gridSize, 
            dungeonDataSo.cubeSize, 
            dungeonDataSo.roomSize, 
            dungeonDataSo.roomSize
        );
        
        // 경로 설정 적용
        generator.pathType = dungeonDataSo.pathType;
        
        return generator;
    }
}
