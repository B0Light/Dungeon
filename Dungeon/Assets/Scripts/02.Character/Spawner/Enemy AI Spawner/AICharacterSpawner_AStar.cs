using System.Collections.Generic;
using UnityEngine;

public class AICharacterSpawner_AStar : MonoBehaviour
{
    [Header("Spawn Settings")] 
    [SerializeField] private GameObject spawnVisual;

    [SerializeField] private SpawnableCharacter spawnPrefab;

    [SerializeField] private Vector3 cellSize = new Vector3(2,2,2);
    [SerializeField] private Vector3 mapOffset = new Vector3(0.5f,2,0.5f);

    private Vector2Int _gridPosition;

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
        _gridPosition = new Vector2Int(gridX, gridY);
        return _gridPosition;
    }
    
    public void SpawnUnit(GridPathfinder pathfinder, List<Vector2Int> patrolPointList)
    {
        GameObject unit = Instantiate(spawnPrefab.characterPrefab);
        
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
        
        // GridMovementController 컴포넌트 추가 및 초기화
        var controller = unit.GetComponent<GridMovementController>();
        if (controller == null)
        {
            controller = unit.AddComponent<GridMovementController>();
        }
        
        controller.cellSize = cellSize;
        controller.gridOffset = mapOffset;
        controller.moveSpeed = 3f;
        controller.allowDiagonalMovement = false;
        
        // 패스파인더와 시작 위치로 초기화
        controller.Initialize(pathfinder, _gridPosition);
        // 정찰 시작 
        controller.StartPatrol(patrolPointList);
    }
}