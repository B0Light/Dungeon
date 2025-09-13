using UnityEngine;

public class BugAICombatManager : AICharacterCombatManager_Range
{
    [SerializeField] private AuraVFXDamageCollider biteAttackDamageCollider;
    
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private int poiseDamage = 10;
    
    private void SetDamageClaw()
    {
        biteAttackDamageCollider.physicalDamage = baseDamage;
        biteAttackDamageCollider.poiseDamage = poiseDamage;
    }
    
    /* Animation Event */
    
    public void AttackBite()
    {
        biteAttackDamageCollider.ownerCharacter = character; 
        biteAttackDamageCollider.EnableDamageCollider(); 
    }
    
    public override void FireProjectile()
    {
        UnifiedProjectilePoolManager.Instance.FireInDirection(aiCharacter, projectileType, firePoint.position, firePoint.forward, firePoint);
    }
}
