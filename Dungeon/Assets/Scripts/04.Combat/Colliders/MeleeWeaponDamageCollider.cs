using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeWeaponDamageCollider : DamageCollider
{
    private Dictionary<AttackType, float> _attackModifiers;
    
    protected override void ModifyDamageEffect(TakeDamageEffect damageEffect)
    {
        
    }

    private void ApplyAttackDamageModifiers(float modifier, TakeDamageEffect damageEffect)
    {
        damageEffect.ApplyAttackDamageModifiers(modifier);
    }
}
