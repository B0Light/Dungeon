using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles particle collision events and spawns effect objects at collision points
/// Based on Unity's MonoBehaviour.OnParticleCollision documentation
/// </summary>
public class ParticleCollisionHandler : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private GameObject[] effectPrefabs;
    [SerializeField] private float effectDestroyDelay = 5f;
    [SerializeField] private bool destroyMainEffectOnCollision = false;
    
    [Header("Position Settings")]
    [SerializeField] private bool useWorldSpacePosition = false;
    [SerializeField] private float normalOffset = 0f;
    
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    [SerializeField] private bool useOnlyRotationOffset = true;
    [SerializeField] private bool useFirePointRotation = false;

    private ParticleSystem _particleSystem;
    private readonly List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();

    #region Unity Lifecycle
    
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        ValidateComponents();
    }

    private void OnParticleCollision(GameObject collidedObject)
    {
        HandleParticleCollisions(collidedObject);
    }

    #endregion

    #region Private Methods

    private void ValidateComponents()
    {
        if (_particleSystem == null)
        {
            Debug.LogError($"ParticleSystem component not found on {gameObject.name}!", this);
            enabled = false;
            return;
        }

        if (effectPrefabs == null || effectPrefabs.Length == 0)
        {
            Debug.LogWarning($"No effect prefabs assigned to {gameObject.name}", this);
        }
    }

    private void HandleParticleCollisions(GameObject collidedObject)
    {
        int collisionCount = _particleSystem.GetCollisionEvents(collidedObject, _collisionEvents);
        
        for (int i = 0; i < collisionCount; i++)
        {
            SpawnEffectsAtCollision(_collisionEvents[i]);
        }

        if (destroyMainEffectOnCollision)
        {
            DestroyMainEffect();
        }
    }

    private void SpawnEffectsAtCollision(ParticleCollisionEvent collisionEvent)
    {
        if (effectPrefabs == null) return;

        Vector3 spawnPosition = CalculateSpawnPosition(collisionEvent);
        Quaternion spawnRotation = CalculateSpawnRotation(collisionEvent);

        foreach (GameObject effectPrefab in effectPrefabs)
        {
            if (effectPrefab == null) continue;

            GameObject effectInstance = Instantiate(effectPrefab, spawnPosition, spawnRotation);
            ConfigureEffectInstance(effectInstance);
            ScheduleDestroy(effectInstance, effectDestroyDelay);
        }
    }

    private Vector3 CalculateSpawnPosition(ParticleCollisionEvent collisionEvent)
    {
        return collisionEvent.intersection + (collisionEvent.normal * normalOffset);
    }

    private Quaternion CalculateSpawnRotation(ParticleCollisionEvent collisionEvent)
    {
        if (useFirePointRotation)
        {
            Vector3 direction = (transform.position - collisionEvent.intersection).normalized;
            return Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
        }

        if (useOnlyRotationOffset && rotationOffset != Vector3.zero)
        {
            return Quaternion.Euler(rotationOffset);
        }

        Quaternion normalRotation = Quaternion.LookRotation(collisionEvent.normal);
        return normalRotation * Quaternion.Euler(rotationOffset);
    }

    private void ConfigureEffectInstance(GameObject instance)
    {
        if (!useWorldSpacePosition)
        {
            instance.transform.SetParent(transform, true);
        }
    }

    private void ScheduleDestroy(GameObject target, float delay)
    {
        if (target != null)
        {
            Destroy(target, delay);
        }
    }

    private void DestroyMainEffect()
    {
        float destroyDelay = effectDestroyDelay + 0.5f;
        Destroy(gameObject, destroyDelay);
    }

    #endregion

    #region Public Methods (Optional - for external control)

    /// <summary>
    /// Manually trigger effect destruction
    /// </summary>
    public void TriggerDestroy()
    {
        DestroyMainEffect();
    }

    /// <summary>
    /// Add effect prefab at runtime
    /// </summary>
    public void AddEffectPrefab(GameObject newEffect)
    {
        if (newEffect == null) return;

        var newArray = new GameObject[effectPrefabs.Length + 1];
        effectPrefabs.CopyTo(newArray, 0);
        newArray[effectPrefabs.Length] = newEffect;
        effectPrefabs = newArray;
    }

    #endregion

    #region Editor Helpers

    #if UNITY_EDITOR
    private void OnValidate()
    {
        effectDestroyDelay = Mathf.Max(0f, effectDestroyDelay);
    }
    #endif

    #endregion
}
