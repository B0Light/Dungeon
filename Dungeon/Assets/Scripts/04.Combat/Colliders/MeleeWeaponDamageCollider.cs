using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeleeWeaponDamageCollider : DamageCollider
{
    private Dictionary<AttackType, float> _attackModifiers;
    
    protected override void ModifyDamageEffect(TakeDamageEffect damageEffect)
    {
        if (_attackModifiers != null && _attackModifiers.TryGetValue(ownerCharacter.characterCombatManager.currentAttackType, out float modifier))
        {
            ApplyAttackDamageModifiers(modifier, damageEffect);
        }
        else
        {
            ApplyAttackDamageModifiers(1, damageEffect);
        }
    }

    private void ApplyAttackDamageModifiers(float modifier, TakeDamageEffect damageEffect)
    {
        damageEffect.ApplyAttackDamageModifiers(modifier);
    }

    public void SetWeaponDamage(CharacterManager characterWieldingWeapon ,EquipmentItemInfoWeapon equipmentItemInfoWeapon)
    {
        ownerCharacter = characterWieldingWeapon;

        physicalDamage = equipmentItemInfoWeapon.GetAbilityValue(ItemEffect.PhysicalAttack);
        magicalDamage = equipmentItemInfoWeapon.GetAbilityValue(ItemEffect.MagicalAttack);
        poiseDamage = equipmentItemInfoWeapon.poiseDamage;
        
        _attackModifiers = new Dictionary<AttackType, float>
        {
            { AttackType.LightAttack01, equipmentItemInfoWeapon.hAtkMod01 },
            { AttackType.LightAttack02, equipmentItemInfoWeapon.hAtkMod02 },
            { AttackType.LightAttack03, equipmentItemInfoWeapon.hAtkMod03 },
            { AttackType.HeavyAttack01, equipmentItemInfoWeapon.vAtkMod01 },
            { AttackType.HeavyAttack02, equipmentItemInfoWeapon.vAtkMod02 },
            { AttackType.HeavyAttack03, equipmentItemInfoWeapon.vAtkMod03 },
            { AttackType.ChargeAttack01, equipmentItemInfoWeapon.vAtkMod01 * 1.5f },
            { AttackType.ChargeAttack02, equipmentItemInfoWeapon.vAtkMod02 * 1.5f },
            { AttackType.ChargeAttack03, equipmentItemInfoWeapon.vAtkMod03 * 1.5f },
            { AttackType.CriticalAttack, equipmentItemInfoWeapon.hAtkMod01 * 5.0f },
            { AttackType.RunningAttack01, equipmentItemInfoWeapon.runningAtkMod },
            { AttackType.RollingAttack01, equipmentItemInfoWeapon.rollingAtkMod },
            { AttackType.BackStepAttack01, equipmentItemInfoWeapon.backStepAtkMod },
            { AttackType.JumpingAttack01, equipmentItemInfoWeapon.jumpingAtkMod }
        };
    }
}
