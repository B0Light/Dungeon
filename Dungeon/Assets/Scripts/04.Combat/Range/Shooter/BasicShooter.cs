using UnityEngine;

public class BasicShooter : MonoBehaviour
{
    [SerializeField] private ProjectileType projectileType = ProjectileType.Bullet;
    [SerializeField] private Transform firePoint;
    
    private UnifiedProjectilePoolManager _poolManager;

    private void Start()
    {
        _poolManager = FindAnyObjectByType<UnifiedProjectilePoolManager>();
        if (firePoint == null) firePoint = transform;
    }

    // 가장 간단한 발사 메서드
    [ContextMenu("Shoot")]
    public void Shoot()
    {
        if (_poolManager != null)
        {
            _poolManager.FireInDirection(null, projectileType, firePoint.position, firePoint.forward, firePoint);
        }
    }

    // 방향 지정 발사
    public void ShootInDirection(Vector3 direction)
    {
        if (_poolManager != null)
        {
            _poolManager.FireInDirection(null, projectileType, firePoint.position, direction, firePoint);
        }
    }

    // 타겟 지정 발사
    public void ShootAtTarget(Transform target)
    {
        if (_poolManager != null && target != null)
        {
            _poolManager.FireAtTarget(null, projectileType, firePoint.position, target, firePoint);
        }
    }
}