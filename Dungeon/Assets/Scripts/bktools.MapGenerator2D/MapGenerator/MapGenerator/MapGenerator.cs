using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private DungeonDataSO _dungeonDataSo;
    private MapGeneratorFactory _generatorFactory;
    private BaseMapGenerator _currentGenerator;

    public void InitGenerator(DungeonDataSO dungeonDataSo)
    {
        _dungeonDataSo = dungeonDataSo;
        _generatorFactory = new MapGeneratorFactory(this.transform, _dungeonDataSo);
    }
    
    public void GenerateMap()
    {
        if (_dungeonDataSo == null)
        {
            Debug.LogError("DungeonDataSO가 할당되지 않았습니다.");
            return;
        }

        SetupCurrentGenerator();

        if (_currentGenerator == null)
        {
            Debug.LogWarning("맵 생성기를 설정할 수 없습니다. DungeonDataSO에 유효한 맵 생성기 타입이 할당되었는지 확인해주세요.");
            return;
        }

        // 시드값이 0이면 새로운 시드 생성, 아니면 고정 시드 사용
        int seed = (_dungeonDataSo.seed == 0) ? System.DateTime.Now.Second : _dungeonDataSo.seed;
        _currentGenerator.GenerateMap(seed);
    }
    
    private void SetupCurrentGenerator()
    {
        _currentGenerator = _generatorFactory.CreateGenerator(_dungeonDataSo.generatorType);
    }

    public MapData GetMapData() => _currentGenerator.GetMapData();

    public Dictionary<RectInt, List<Vector2Int>> GetRoomDirection() => _currentGenerator.RoomGateDirections;

}