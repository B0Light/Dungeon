public interface IMapGenerator
{
    FixedGridXZ<GridCell> GenerateMap(int seed);
    
    bool IsMapGenerated { get; }
    
    MapData GetMapData();
}
