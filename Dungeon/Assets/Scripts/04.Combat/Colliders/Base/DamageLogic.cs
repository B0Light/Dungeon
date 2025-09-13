using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public abstract class DamageLogic : MonoBehaviour
{
    [HideInInspector] public CharacterManager ownerCharacter;

    [HideInInspector] public float physicalDamage = 0;
    [HideInInspector] public float magicalDamage = 0;
    [HideInInspector] public float poiseDamage = 0;

    [Header("Contact Point")]
    protected Vector3 contactPoint;
    
    [Header("BLOCK")]
    protected float _dotValueFromAttackToDamageTarget;

    [Header("Characters Damaged")]
    protected readonly List<CharacterManager> charactersDamaged = new List<CharacterManager>();

    protected virtual void Awake()
    {
        ownerCharacter = GetComponentInParent<CharacterManager>();
    }

    protected void SetBlockingDotValues(CharacterManager damageTarget)
    {
        if (damageTarget == null || ownerCharacter == null) return;
        
        Vector3 directionFromAttackToDamageTarget = ownerCharacter.transform.position - damageTarget.transform.position;
        _dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
    }
    
    protected bool CheckForParried(CharacterManager damageTarget)
    {
        if (_dotValueFromAttackToDamageTarget < 0.3f) return false;
        // 패링했는지 확인
        if (damageTarget.characterVariableManager.isParring.Value == false) return false;
        
        charactersDamaged.Add(damageTarget);
        
        WorldCharacterEffectsManager.Instance.OnParrySuccess(contactPoint);
        damageTarget.characterVariableManager.SuccessParry(ownerCharacter); // 무적효과 
        damageTarget.characterAnimatorManager.PlayTargetActionAnimation("Parry", true);
        damageTarget.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.ChooseParriedSfx());
        return true;
    }
    
    protected bool CheckForBlock(CharacterManager damageTarget)
    {
        // 블록했는지 확인
        if (damageTarget.characterVariableManager.isBlock.Value == false) return false;
        // 블록이 가능한지 확인
        if (_dotValueFromAttackToDamageTarget < 0.5f) return false;
        // 블록할 액션포인트가 있는지 확인 -> 있음 
        if (damageTarget.characterStatsManager.UseActionPoint()) return true;
        // 없음 -> 블록 실패 
        damageTarget.characterAnimatorManager.PlayTargetActionAnimation("Break", true);
        return false;
    }

    protected void DamageTarget(CharacterManager damageTarget, bool isBlock = false)
    {
        if (charactersDamaged.Contains(damageTarget))
            return;
        
        charactersDamaged.Add(damageTarget);

        TakeDamageEffect damageEffect = 
            Instantiate(isBlock ? 
                WorldCharacterEffectsManager.Instance.takeBlockDamageEffect :
                WorldCharacterEffectsManager.Instance.takeDamageEffect);
        
        damageEffect.SetDamage(
            ownerCharacter,
            physicalDamage, 
            magicalDamage, 
            ownerCharacter.characterStatsManager.extraDamage.Value, 
            poiseDamage, 
            contactPoint,
            Vector3.SignedAngle(ownerCharacter.transform.forward, damageTarget.transform.forward, Vector3.up)
            );
        
        Debug.LogWarning("DAMAGE INFO :" + damageEffect);
        
        ModifyDamageEffect(damageEffect);
        damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }

    protected virtual void ModifyDamageEffect(TakeDamageEffect damageEffect) { }
}