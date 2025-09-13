using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class UnifiedProjectilePoolManager : Singleton<UnifiedProjectilePoolManager>
{
    [SerializeField] private List<ProjectileConfiguration> projectileConfigurations = new List<ProjectileConfiguration>();
    
    private readonly Dictionary<ProjectileType, IObjectPool<HitscanProjectile>> _hitScanPools = 
        new Dictionary<ProjectileType, IObjectPool<HitscanProjectile>>();
    private readonly Dictionary<ProjectileType, IObjectPool<PhysicalProjectile>> _physicalPools = 
        new Dictionary<ProjectileType, IObjectPool<PhysicalProjectile>>();
    private readonly Dictionary<ProjectileType, IObjectPool<GuidedProjectile>> _guidedPools = 
        new Dictionary<ProjectileType, IObjectPool<GuidedProjectile>>();
    private readonly Dictionary<ProjectileType, IObjectPool<EffectPlayer>> _impactParticlePools = 
        new Dictionary<ProjectileType, IObjectPool<EffectPlayer>>();
    private readonly Dictionary<ProjectileType, IObjectPool<EffectPlayer>> _effectObjectPools = 
        new Dictionary<ProjectileType, IObjectPool<EffectPlayer>>();
    private readonly Dictionary<ProjectileType, ProjectileConfiguration> _configLookup = 
        new Dictionary<ProjectileType, ProjectileConfiguration>();

    private Dictionary<ProjectileBehavior, Action<ProjectileType, ProjectileFireData>> _fireStrategies;

    protected override void Awake()
    {
        base.Awake();
        InitializeFireStrategies();
        InitializePools();
    }

    #region 초기화

    private void InitializeFireStrategies()
    {
        _fireStrategies = new Dictionary<ProjectileBehavior, Action<ProjectileType, ProjectileFireData>>
        {
            { ProjectileBehavior.HitScan, FireHitScanProjectile },
            { ProjectileBehavior.Physical, FirePhysicalProjectile },
            { ProjectileBehavior.Guided, FireGuidedProjectile }
        };
    }

    private void InitializePools()
    {
        foreach (var config in projectileConfigurations)
        {
            if (!ValidateConfiguration(config))
                continue;

            _configLookup[config.projectileType] = config;
            CreateProjectilePool(config);
            CreateParticlePoolIfNeeded(config);
            CreateEffectObjectPoolIfNeeded(config);
        }
    }

    private bool ValidateConfiguration(ProjectileConfiguration config)
    {
        if (config.projectilePrefab == null)
        {
            Debug.LogError($"Projectile prefab for {config.projectileType} is not assigned!");
            return false;
        }
        return true;
    }

    private void CreateProjectilePool(ProjectileConfiguration config)
    {
        switch (config.behavior)
        {
            case ProjectileBehavior.HitScan:
                CreateHitScanPool(config);
                break;
            case ProjectileBehavior.Physical:
                CreatePhysicalPool(config);
                break;
            case ProjectileBehavior.Guided:
                CreateGuidedPool(config);
                break;
            default:
                Debug.LogWarning($"Unsupported projectile behavior: {config.behavior}");
                break;
        }
    }

    private void CreateParticlePoolIfNeeded(ProjectileConfiguration config)
    {
        if (config.impactParticlePrefab != null)
        {
            CreateImpactParticlePool(config);
        }
    }
    
    private void CreateEffectObjectPoolIfNeeded(ProjectileConfiguration config)
    {
        if (config.effectObjectPrefab != null)
        {
            CreateEffectObjectPool(config);
        }
    }

    #endregion

    #region 풀 생성 메서드들

    private void CreateHitScanPool(ProjectileConfiguration config)
    {
        var pool = CreateGenericPool<HitscanProjectile>(
            () => CreateHitScanProjectile(config),
            config.initialPoolSize
        );
        _hitScanPools.Add(config.projectileType, pool);
    }

    private void CreatePhysicalPool(ProjectileConfiguration config)
    {
        var pool = CreateGenericPool<PhysicalProjectile>(
            () => CreatePhysicalProjectile(config),
            config.initialPoolSize
        );
        _physicalPools.Add(config.projectileType, pool);
    }

    private void CreateGuidedPool(ProjectileConfiguration config)
    {
        var pool = CreateGenericPool<GuidedProjectile>(
            () => CreateGuidedProjectile(config),
            config.initialPoolSize
        );
        _guidedPools.Add(config.projectileType, pool);
    }

    private void CreateImpactParticlePool(ProjectileConfiguration config)
    {
        var pool = CreateGenericPool<EffectPlayer>(
            () => CreateImpactParticleInstance(config.impactParticlePrefab).GetComponent<EffectPlayer>(),
            config.initialPoolSize
        );
        _impactParticlePools.Add(config.projectileType, pool);
    }
    
    private void CreateEffectObjectPool(ProjectileConfiguration config)
    {
        var pool = CreateGenericPool<EffectPlayer>(
            () => CreateEffectObjectInstance(config.effectObjectPrefab).GetComponent<EffectPlayer>(),
            config.initialPoolSize
        );
        _effectObjectPools.Add(config.projectileType, pool);
    }

    // 제네릭 풀 생성 헬퍼
    private IObjectPool<T> CreateGenericPool<T>(Func<T> createFunc, int initialPoolSize) where T : Component
    {
        return new ObjectPool<T>(
            createFunc: createFunc,
            actionOnGet: (item) => item.gameObject.SetActive(true),
            actionOnRelease: (item) => 
            {
                item.gameObject.SetActive(false);
                item.transform.SetParent(transform);
            },
            actionOnDestroy: (item) => Destroy(item.gameObject),
            defaultCapacity: initialPoolSize,
            maxSize: initialPoolSize * 3
        );
    }

    #endregion

    #region 프로젝타일 생성 메서드들

    private HitscanProjectile CreateHitScanProjectile(ProjectileConfiguration config)
    {
        var projectile = CreateProjectileInstance<HitscanProjectile>(config);
        projectile.SetPool(_hitScanPools[config.projectileType]);
        return projectile;
    }

    private PhysicalProjectile CreatePhysicalProjectile(ProjectileConfiguration config)
    {
        var projectile = CreateProjectileInstance<PhysicalProjectile>(config);
        
        _impactParticlePools.TryGetValue(config.projectileType, out IObjectPool<EffectPlayer> impactParticlePool);
        _effectObjectPools.TryGetValue(config.projectileType, out IObjectPool<EffectPlayer> effectObjectPool);
        projectile.SetPools(_physicalPools[config.projectileType], impactParticlePool, effectObjectPool);
        
        return projectile;
    }

    private GuidedProjectile CreateGuidedProjectile(ProjectileConfiguration config)
    {
        var projectile = CreateProjectileInstance<GuidedProjectile>(config);
        projectile.SetPool(_guidedPools[config.projectileType]);
        return projectile;
    }

    // 제네릭 프로젝타일 인스턴스 생성 헬퍼
    private T CreateProjectileInstance<T>(ProjectileConfiguration config) where T : Component, IProjectile
    {
        GameObject instance = Instantiate(config.projectilePrefab, transform);
        T projectile = instance.GetComponent<T>() ?? instance.AddComponent<T>();
        
        projectile.Initialize(config);
        instance.SetActive(false);
        
        return projectile;
    }

    private GameObject CreateImpactParticleInstance(EffectPlayer prefab)
    {
        GameObject instance = Instantiate(prefab.gameObject, transform);
        instance.SetActive(false);
        return instance;
    }
    
    private GameObject CreateEffectObjectInstance(EffectPlayer prefab)
    {
        GameObject instance = Instantiate(prefab.gameObject, transform);
        instance.SetActive(false);
        return instance;
    }

    #endregion

    #region 발사 데이터 구조체

    private readonly struct ProjectileFireData
    {
        public readonly CharacterManager Owner;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly Transform FirePoint;
        public readonly Transform Target;
        public readonly Vector3 TargetOffset;

        public ProjectileFireData(CharacterManager owner, Vector3 position, Vector3 direction, Transform firePoint, Transform target = null, Vector3 targetOffset = default)
        {
            Owner = owner;
            Position = position;
            Direction = direction;
            FirePoint = firePoint;
            Target = target;
            TargetOffset = targetOffset;
        }
    }

    #endregion

    #region 공용 발사 메서드들

    public void FireAtTarget(CharacterManager owner, ProjectileType projectileType, Vector3 firePosition, Transform target, Transform firePoint)
    {
        if (!ValidateFireAtTarget(target, projectileType))
            return;

        var config = _configLookup[projectileType];
        
        if (config.behavior == ProjectileBehavior.Guided)
        {
            FireGuidedAtTarget(owner, projectileType, firePosition, target, firePoint);
        }
        else
        {
            Vector3 direction = (target.position - firePosition).normalized;
            FireProjectile(owner, projectileType, firePosition, direction, firePoint);
        }
    }

    public void FireGuidedAtTarget(CharacterManager owner, ProjectileType projectileType, Vector3 firePosition, Transform target, Transform firePoint)
    {
        if (!TryGetProjectileFromPool(_guidedPools, projectileType, out GuidedProjectile projectile))
            return;

        var fireData = new ProjectileFireData(owner, firePosition, Vector3.zero, firePoint, target, Vector3.up * 1f);
        InitializeGuidedProjectile(projectile, fireData);
    }

    public void FireInDirection(CharacterManager owner, ProjectileType projectileType, Vector3 firePosition, Vector3 direction, Transform firePoint)
    {
        FireProjectile(owner, projectileType, firePosition, direction.normalized, firePoint);
    }

    public void FireShotgun(CharacterManager owner, ProjectileType projectileType, Vector3 firePosition, Vector3 direction, 
                           int pelletCount, float spreadAngle, Transform firePoint = null)
    {
        if (!ValidateShotgunFire(direction))
            return;
        
        Vector3 normalizedDirection = direction.normalized;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 spreadDir = CalculateSpreadDirection(normalizedDirection, spreadAngle);
            FireProjectile(owner, projectileType, firePosition, spreadDir, firePoint);
        }
    }
    
    public void FireGuidedBurst(CharacterManager owner, ProjectileType projectileType, Vector3 firePosition, 
                               Transform target, Transform firePoint, int burstCount = 5, float interval = 0.1f)
    {
        if (!ValidateGuidedBurst(target))
            return;
        
        StartCoroutine(GuidedBurstCoroutine(owner, projectileType, firePosition, target, firePoint, burstCount, interval));
    }

    #endregion

    #region 핵심 발사 로직

    private void FireProjectile(CharacterManager owner, ProjectileType projectileType, Vector3 position, Vector3 direction, Transform firePoint)
    {
        if (!TryGetProjectileConfig(projectileType, out ProjectileConfiguration config))
            return;

        var projectileData = new ProjectileFireData(owner, position, direction, firePoint);
        FireProjectileByBehavior(config.behavior, projectileType, projectileData);
    }

    private bool TryGetProjectileConfig(ProjectileType projectileType, out ProjectileConfiguration config)
    {
        if (_configLookup.TryGetValue(projectileType, out config))
            return true;
            
        Debug.LogError($"No configuration found for projectile type: {projectileType}");
        return false;
    }

    private void FireProjectileByBehavior(ProjectileBehavior behavior, ProjectileType projectileType, ProjectileFireData fireData)
    {
        if (_fireStrategies.TryGetValue(behavior, out var strategy))
        {
            strategy(projectileType, fireData);
        }
        else
        {
            Debug.LogWarning($"Unsupported projectile behavior: {behavior}");
        }
    }

    // 각 타입별 발사 메서드들
    private void FireHitScanProjectile(ProjectileType projectileType, ProjectileFireData fireData)
    {
        if (!TryGetProjectileFromPool(_hitScanPools, projectileType, out HitscanProjectile projectile))
            return;
            
        InitializeAndFireProjectile(projectile, fireData);
    }

    private void FirePhysicalProjectile(ProjectileType projectileType, ProjectileFireData fireData)
    {
        if (!TryGetProjectileFromPool(_physicalPools, projectileType, out PhysicalProjectile projectile))
            return;
            
        InitializeAndFireProjectile(projectile, fireData);
    }

    private void FireGuidedProjectile(ProjectileType projectileType, ProjectileFireData fireData)
    {
        if (!TryGetProjectileFromPool(_guidedPools, projectileType, out GuidedProjectile projectile))
            return;
            
        InitializeAndFireProjectile(projectile, fireData);
    }

    // 제네릭 헬퍼 메서드들
    private bool TryGetProjectileFromPool<T>(Dictionary<ProjectileType, IObjectPool<T>> pools, ProjectileType projectileType, out T projectile) 
        where T : class
    {
        if (pools.TryGetValue(projectileType, out IObjectPool<T> pool))
        {
            projectile = pool.Get();
            return projectile != null;
        }
        
        projectile = null;
        Debug.LogError($"No pool found for projectile type: {projectileType}");
        return false;
    }

    private void InitializeAndFireProjectile<T>(T projectile, ProjectileFireData fireData) 
        where T : IProjectile
    {
        projectile.SetOwner(fireData.Owner);
        projectile.Fire(fireData.Position, fireData.Direction, fireData.FirePoint);
    }

    private void InitializeGuidedProjectile(GuidedProjectile projectile, ProjectileFireData fireData)
    {
        projectile.SetOwner(fireData.Owner);
        projectile.SetTarget(fireData.Target, fireData.TargetOffset);
        
        Vector3 initialDirection = (fireData.Target.position - fireData.Position).normalized;
        projectile.Fire(fireData.Position, initialDirection, fireData.FirePoint);
    }

    #endregion

    #region 유효성 검사 및 헬퍼 메서드들

    private bool ValidateFireAtTarget(Transform target, ProjectileType projectileType)
    {
        if (target == null)
        {
            Debug.LogWarning("Cannot fire at null target!");
            return false;
        }
        
        return TryGetProjectileConfig(projectileType, out _);
    }

    private bool ValidateShotgunFire(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("Cannot fire shotgun in zero direction!");
            return false;
        }
        return true;
    }

    private bool ValidateGuidedBurst(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Cannot fire guided burst at null target!");
            return false;
        }
        return true;
    }

    private Vector3 CalculateSpreadDirection(Vector3 baseDirection, float spreadAngle)
    {
        return Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle), 
            Random.Range(-spreadAngle, spreadAngle), 
            0f) * baseDirection;
    }

    #endregion

    #region 코루틴

    private IEnumerator GuidedBurstCoroutine(CharacterManager owner, ProjectileType projectileType, Vector3 firePosition, 
                                           Transform target, Transform firePoint, int burstCount, float interval)
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (target != null) // 타겟이 살아있는지 확인
            {
                FireGuidedAtTarget(owner, projectileType, firePosition, target, firePoint);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    #endregion
}