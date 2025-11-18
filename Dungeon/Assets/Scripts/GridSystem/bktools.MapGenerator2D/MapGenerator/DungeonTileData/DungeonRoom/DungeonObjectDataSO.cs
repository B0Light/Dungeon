using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Editor/ObjectData ")]
public class DungeonObjectDataSO : ScriptableObject
{
    public BuildObjData buildObject;
    public Vector2Int pos;
    public BuildObjData.Dir dir;
    public int level;
}
