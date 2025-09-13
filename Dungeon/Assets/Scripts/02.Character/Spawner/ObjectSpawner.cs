using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Object")] 
    [SerializeField] private GameObject gameObjectPrefab;
    [SerializeField] private GameObject instantiatedGameObject;
    
    private void Start()
    {
        WorldObjectManager.Instance.SpawnObject(this);
        gameObject.SetActive(false);
    }
    
    public void AttemptToSpawnCharacter()
    {
        if (gameObjectPrefab != null)
        {
            
            instantiatedGameObject = Instantiate(gameObjectPrefab);
            instantiatedGameObject.transform.position = transform.position;
            instantiatedGameObject.transform.rotation = transform.rotation;
        }
    }
}
