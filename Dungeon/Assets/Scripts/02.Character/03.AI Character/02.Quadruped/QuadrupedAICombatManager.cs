using System;
using UnityEngine;

public class QuadrupedAICombatManager : AICharacterCombatManager_Range
{
    private QuadrupedAIManager _quadrupedAIManager;
    [SerializeField] private AuraVFXDamageCollider biteAttackDamageCollider;
    [SerializeField] private AuraVFXDamageCollider clawAttackDamageCollider;

    [SerializeField] private int baseDamage = 10;
    [SerializeField] private int poiseDamage = 10;
 
    private void Start()
    {
        _quadrupedAIManager = character as QuadrupedAIManager;

        SetDamageBite();
        SetDamageClaw();
    }

    private void SetDamageClaw()
    {
        clawAttackDamageCollider.physicalDamage = baseDamage;
        clawAttackDamageCollider.poiseDamage = poiseDamage;
    }
    
    private void SetDamageBite()
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
    
    public void AttackFrontLeg()
    {
        clawAttackDamageCollider.ownerCharacter = character; 
        clawAttackDamageCollider.EnableDamageCollider(); 
    }
}
