using UnityEngine;

// 기본 투사체 인터페이스
public interface IProjectile
{
    void SetOwner(CharacterManager owner);
    void Initialize(ProjectileConfiguration config);
    void Fire(Vector3 position, Vector3 direction, Transform firePoint);
    void ReturnToPool();
}
