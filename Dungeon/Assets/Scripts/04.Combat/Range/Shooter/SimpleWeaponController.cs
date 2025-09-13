using UnityEngine;

public class SimpleWeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private ProjectileType projectileType = ProjectileType.Bullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f; // 초당 발사 횟수
    [SerializeField] private bool autoFire = false; // 자동 발사 여부
    
    [Header("Shotgun Settings")]
    [SerializeField] private bool isShotgun = false;
    [SerializeField] private int pelletCount = 5;
    [SerializeField] private float spreadAngle = 15f;
    
    private UnifiedProjectilePoolManager _poolManager;
    private float _lastFireTime;
    private Camera _playerCamera;

    private void Awake()
    {
        // Pool Manager 찾기
        _poolManager = FindAnyObjectByType<UnifiedProjectilePoolManager>();
        if (_poolManager == null)
        {
            Debug.LogError("ParticleProjectilePoolManager not found in scene!");
        }

        // Fire Point 설정 (없으면 현재 오브젝트 사용)
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // 플레이어 카메라 찾기 (방향 계산용)
        _playerCamera = Camera.main;
        if (_playerCamera == null)
        {
            _playerCamera = FindFirstObjectByType<Camera>();
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (autoFire)
        {
            // 자동 발사 모드
            if (Input.GetButton("Fire1") && CanFire())
            {
                Fire();
            }
        }
        else
        {
            // 단발 발사 모드
            if (Input.GetButtonDown("Fire1") && CanFire())
            {
                Fire();
            }
        }
    }

    private bool CanFire()
    {
        return Time.time >= _lastFireTime + (1f / fireRate);
    }

    public void Fire()
    {
        if (_poolManager == null) return;

        _lastFireTime = Time.time;
        Vector3 firePosition = firePoint.position;
        Vector3 fireDirection = GetFireDirection();

        if (isShotgun)
        {
            _poolManager.FireShotgun(null, projectileType, firePosition, fireDirection, pelletCount, spreadAngle);
        }
        else
        {
            _poolManager.FireInDirection(null, projectileType, firePosition, fireDirection, firePoint);
        }

        // 발사 사운드나 이펙트를 여기서 추가할 수 있습니다
        OnWeaponFired();
    }

    public void FireAtTarget(Transform target)
    {
        if (_poolManager == null || target == null) return;

        _lastFireTime = Time.time;
        Vector3 firePosition = firePoint.position;

        _poolManager.FireAtTarget(null, projectileType, firePosition, target,  firePoint);
        OnWeaponFired();
    }

    public void FireInCustomDirection(Vector3 direction)
    {
        if (_poolManager == null) return;

        _lastFireTime = Time.time;
        Vector3 firePosition = firePoint.position;

        if (isShotgun)
        {
            _poolManager.FireShotgun(null, projectileType, firePosition, direction, pelletCount, spreadAngle, firePoint);
        }
        else
        {
            _poolManager.FireInDirection(null, projectileType, firePosition, direction, firePoint);
        }

        OnWeaponFired();
    }

    private Vector3 GetFireDirection()
    {
        // 카메라가 있으면 카메라 방향 사용
        if (_playerCamera != null)
        {
            return _playerCamera.transform.forward;
        }
        
        // 없으면 오브젝트의 forward 방향 사용
        return firePoint.forward;
    }

    private void OnWeaponFired()
    {
        // 발사 시 호출되는 이벤트 메서드
        // 여기서 사운드, 이펙트, 애니메이션 등을 처리할 수 있습니다
        Debug.Log($"Weapon fired: {projectileType}");
    }

    // 외부에서 무기 설정을 변경할 수 있는 메서드들
    public void SetProjectileType(ProjectileType newType)
    {
        projectileType = newType;
    }

    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.1f, newFireRate);
    }

    public void SetAutoFire(bool enabled)
    {
        autoFire = enabled;
    }

    public void SetShotgunMode(bool enabled, int pellets = 5, float spread = 15f)
    {
        isShotgun = enabled;
        pelletCount = pellets;
        spreadAngle = spread;
    }
}