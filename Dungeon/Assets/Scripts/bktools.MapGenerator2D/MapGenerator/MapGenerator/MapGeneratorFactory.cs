using UnityEngine;

public class MapGeneratorFactory
{
    private readonly Transform _slot;
    private readonly DungeonDataSO _dungeonDataSo;

    public MapGeneratorFactory(Transform slot, DungeonDataSO dungeonDataSo)
    {
        _slot = slot;
        _dungeonDataSo = dungeonDataSo;
    }

    public BaseMapGenerator CreateGenerator(MapGeneratorType type)
    {
        switch (type)
        {
            case MapGeneratorType.BSP:
                return new BSPDungeonMapGenerator(_slot, _dungeonDataSo);
            case MapGeneratorType.BSPFull:
                return new BSPDungeonMapGeneratorFull(_slot, _dungeonDataSo);
            case MapGeneratorType.Isaac:
                return new IsaacMapGenerator(_slot, _dungeonDataSo);
            case MapGeneratorType.Delaunay:
                return new DelaunayMapGenerator(_slot, _dungeonDataSo);
            default:
                Debug.LogError($"알 수 없는 맵 생성기 타입: {type}");
                return null;
        }
    }
}