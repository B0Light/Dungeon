using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Dungeon/Place ")]
public class DungeonPlaceData : ScriptableObject
{
    public List<DungeonData> dungeonDataList;
}
