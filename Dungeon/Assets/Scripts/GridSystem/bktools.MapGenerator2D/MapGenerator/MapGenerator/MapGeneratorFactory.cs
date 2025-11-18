using UnityEngine;

public class MapGeneratorFactory
{
    private readonly Transform _slot;
    private readonly DungeonGenerateDataSO _dungeonGenerateDataSo;

    public MapGeneratorFactory(Transform slot, DungeonGenerateDataSO dungeonGenerateDataSo)
    {
        _slot = slot;
        _dungeonGenerateDataSo = dungeonGenerateDataSo;
    }

    public BaseMapGenerator CreateGenerator(MapGeneratorType type)
    {
        switch (type)
        {
            case MapGeneratorType.BSP:
                return new BSPDungeonMapGenerator(_slot, _dungeonGenerateDataSo);
            case MapGeneratorType.BSPFull:
                return new BSPDungeonMapGeneratorFull(_slot, _dungeonGenerateDataSo);
            case MapGeneratorType.Isaac:
                return new IsaacMapGenerator(_slot, _dungeonGenerateDataSo);
            case MapGeneratorType.Delaunay:
                return new DelaunayMapGenerator(_slot, _dungeonGenerateDataSo);
            default:
                Debug.LogError($"알 수 없는 맵 생성기 타입: {type}");
                return null;
        }
    }
}