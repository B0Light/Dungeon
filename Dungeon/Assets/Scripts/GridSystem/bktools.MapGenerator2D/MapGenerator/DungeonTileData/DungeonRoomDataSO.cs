using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/RoomData ")]
public class DungeonRoomDataSO : ScriptableObject
{
    public List<GameObject> essentialBuilding;
    public List<GameObject> subBuilding;
}
