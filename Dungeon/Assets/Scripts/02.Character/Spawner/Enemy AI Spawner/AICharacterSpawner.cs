using UnityEngine;

public class AICharacterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")] 
    [SerializeField] private GameObject spawnVisual;
    
    private GameObject instantiatedGameObject;
    private float totalWeight;
    private SpawnAICharacterSO _spawnableCharacters;

    private void Start()
    {
        _spawnableCharacters = AISpawnManager.Instance.spawnableCharacters;
        CalculateTotalWeight();
        AISpawnManager.Instance.RegisterSpawner(this);
        if (spawnVisual != null)
            spawnVisual.SetActive(false);
    }

    private void CalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (var character in _spawnableCharacters.spawnData)
        {
            if (character.characterPrefab != null)
                totalWeight += character.spawnWeight;
        }
    }

    public bool AttemptToSpawnCharacter(bool isBoss = false)
    {
        if ((isBoss ? _spawnableCharacters.stageBossData == null : _spawnableCharacters.spawnData.Count == 0) )
        {
            Debug.LogWarning(isBoss ? "NO BOSS" : "No spawnable characters configured!");
            return false;
        }

        if (isBoss && _spawnableCharacters.stageBossData != null)
        {
            SpawnCharacter(_spawnableCharacters.stageBossData);
            return true;
        }
        else
        {
            SpawnableCharacter selectedCharacter = SelectRandomCharacter();
            if (selectedCharacter?.characterPrefab != null)
            {
                SpawnCharacter(selectedCharacter);
                return true;
            }
        }
        
        return false;
    }

    private SpawnableCharacter SelectRandomCharacter()
    {
        if (totalWeight <= 0)
        {
            CalculateTotalWeight();
            if (totalWeight <= 0) return null;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var character in _spawnableCharacters.spawnData)
        {
            if (character.characterPrefab == null) continue;
            
            currentWeight += character.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return character;
            }
        }

        // 예외적으로 첫 번째 유효한 캐릭터 반환
        foreach (var character in _spawnableCharacters.spawnData)
        {
            if (character.characterPrefab != null)
                return character;
        }

        return null;
    }

    private void SpawnCharacter(SpawnableCharacter character)
    {
        instantiatedGameObject = Instantiate(character.characterPrefab, transform.position, transform.rotation);
        
        if (character.maxHealth > 0)
        {
            AICharacterVariableManager aiCharacterVariableManager = instantiatedGameObject.GetComponent<AICharacterVariableManager>();
            if (aiCharacterVariableManager != null)
            {
                aiCharacterVariableManager.SetInitialMaxHealth(character.maxHealth);
                aiCharacterVariableManager.InitVariable();
                aiCharacterVariableManager.health.MaxValue = character.maxHealth;
            }
        }

        AICharacterManager characterManager = instantiatedGameObject.GetComponent<AICharacterManager>();
        if (characterManager != null)
        {
            AISpawnManager.Instance.AddCharacterToSpawnedCharactersList(characterManager);
        }
    }
}