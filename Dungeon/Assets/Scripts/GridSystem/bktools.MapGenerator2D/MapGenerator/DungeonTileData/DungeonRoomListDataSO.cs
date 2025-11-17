using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Dungeon/Editor/RoomListData ")]
public class DungeonRoomListDataSO : ScriptableObject
{
    public List<DungeonRoomDataSO> essentialRoom;
    public List<DungeonRoomDataSO> subRoom;
}

[CreateAssetMenu(menuName = "Dungeon/Editor/RoomData ")]
public class DungeonRoomDataSO : ScriptableObject
{
    public List<DungeonObjectDataSO> props;
}

[CreateAssetMenu(menuName = "Dungeon/Editor/ObjectData ")]
public class DungeonObjectDataSO : ScriptableObject
{
    public BuildObjData buildObject;
    public Vector2Int pos;
    public BuildObjData.Dir dir;
    public int level;
}