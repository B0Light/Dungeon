using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldObjectManager : Singleton<WorldObjectManager>
{
    
    [Header("Network Objects")] 
    [SerializeField] List<ObjectSpawner> networkObjectSpawners;
    [SerializeField] List<GameObject> spawnedInObjects;

    [Header("Fog Walls")] 
    public List<FogWallInteractable> fogWalls;

    
    public void SpawnObject(ObjectSpawner objectSpawner)
    {
        
        networkObjectSpawners.Add(objectSpawner);
        objectSpawner.AttemptToSpawnCharacter();

    }

    public void AddFogWallToList(FogWallInteractable fogWall)
    {
        if (!fogWalls.Contains(fogWall))
        {
            fogWalls.Add(fogWall);
        }
    }
    
    public void RemoveFogWallToList(FogWallInteractable fogWall)
    {
        if (fogWalls.Contains(fogWall))
        {
            fogWalls.Remove(fogWall);
        }
    }
}
