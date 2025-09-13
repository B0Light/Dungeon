using UnityEngine;

public class BossAICombatManager : AICharacterCombatManager
{
    [SerializeField] private ProjectileType projectileType;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform smashPoint;
    
    [SerializeField] private ProjectileType guidedMissileType; // 인스펙터에서 설정
 
    private void Start()
    {
        if (firePoint == null) firePoint = transform;
    }

    public override void FindTargetViaLineOfSight(AICharacterManager curCharacter)
    {
        currentTarget = GameManager.Instance.GetPlayer();
    }


    /* Animation Event */
    public void AttackSmash()
    {
        UnifiedProjectilePoolManager.Instance.FireInDirection(
            aiCharacter,
            ProjectileType.BossSmash, 
            smashPoint.position, 
            Vector3.up,
            smashPoint
        );
    }
    
    public void AttackJump()
    {
        UnifiedProjectilePoolManager.Instance.FireInDirection(
            aiCharacter,
            ProjectileType.BossJumpAttack, 
            transform.position, 
            Vector3.zero,
            transform
        );
    }
    
    // Base Attack
    public void FireProjectile()
    {
        UnifiedProjectilePoolManager.Instance.FireAtTarget(aiCharacter, projectileType, transform.position, currentTarget.transform, transform);
    }
    
    public void FireGuidedAttack()
    {
        // 단발 유도 탄환
        UnifiedProjectilePoolManager.Instance.FireAtTarget(
            aiCharacter, 
            guidedMissileType, 
            firePoint.position, 
            currentTarget.transform, 
            firePoint
        );
    }

    public void FireGuidedBurst()
    {
        // 연사 유도 탄환 (5발, 0.1초 간격)
        UnifiedProjectilePoolManager.Instance.FireGuidedBurst(
            aiCharacter, 
            guidedMissileType, 
            firePoint.position, 
            currentTarget.transform, 
            firePoint, 
            5, 
            0.1f
        );
    }
}

