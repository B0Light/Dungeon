public interface IMapGenerator
{
    void GenerateMap(int seed);
    
    bool IsMapGenerated { get; }
    
    MapData GetMapData();
}
