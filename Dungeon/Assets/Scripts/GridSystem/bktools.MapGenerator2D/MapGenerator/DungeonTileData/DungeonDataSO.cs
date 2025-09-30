using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Data ")]
public class DungeonDataSO : ScriptableObject
{
    public string dungeonCode;
    public int seed;
    public MapGeneratorType generatorType;
    public Vector2Int gridSize;
    public Vector3 cubeSize;
    [Range(5, 15)] public int roomSize;
    public PathType pathType;
    public TileMappingDataSO tileMappingDataSO;
}
