using System.Collections.Generic;
using UnityEngine;

public class AICharacterSpawner_Navmesh : MonoBehaviour
{
    [Header("Spawn Settings")] 
    [SerializeField] private GameObject spawnVisual;

    [SerializeField] private SpawnableCharacter spawnPrefab;
    
    [SerializeField] private Vector3 cellSize = new Vector3(2,2,2);
    [SerializeField] private Vector3 mapOffset = new Vector3(0.5f,2,0.5f);
    
    private void Start()
    {
        AISpawnManager.Instance.RegisterSpawner(this);
        if (spawnVisual != null)
            spawnVisual.SetActive(false);
    }
    
    public Vector2Int Init()
    {
        int gridX = Mathf.FloorToInt(transform.position.x / cellSize.x);
        int gridY = Mathf.FloorToInt(transform.position.z / cellSize.y);
        return new Vector2Int(gridX, gridY);
    }
    
    public void SpawnUnit(List<Vector2Int> patrolPointList)
    {
        GameObject unit = Instantiate(spawnPrefab.characterPrefab, transform);
        unit.transform.SetParent(null);
        if (spawnPrefab.maxHealth > 0)
        {
            AICharacterVariableManager aiCharacterVariableManager = unit.GetComponent<AICharacterVariableManager>();
            if (aiCharacterVariableManager != null)
            {
                aiCharacterVariableManager.SetInitialMaxHealth(spawnPrefab.maxHealth);
                aiCharacterVariableManager.InitVariable();
                aiCharacterVariableManager.health.MaxValue = spawnPrefab.maxHealth;
            }
        }

        AICharacterManager characterManager = unit.GetComponent<AICharacterManager>();
        if (characterManager != null)
        {
            AISpawnManager.Instance.AddCharacterToSpawnedCharactersList(characterManager);
        }

        AICharacterPatrolManager patrolManager = unit.GetComponent<AICharacterPatrolManager>();
        
        patrolManager.cellSize = cellSize;
        patrolManager.gridOffset = mapOffset;
        patrolManager.SetPatrolPoint(patrolPointList, Init());

    }
}
