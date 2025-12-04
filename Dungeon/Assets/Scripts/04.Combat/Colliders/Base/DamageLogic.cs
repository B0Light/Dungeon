using System.Collections.Generic;
using UnityEngine;

public enum MagicalType
{
    Fire,
    Water,
    Grass,
    Dark,
    Light,
}

public abstract class DamageLogic : MonoBehaviour
{
    [HideInInspector] public CharacterManager ownerCharacter;

    [Header("Contact Point")]
    protected Vector3 contactPoint;
    
    [Header("BLOCK")]
    protected float _dotValueFromAttackToDamageTarget;

    [Header("Characters Damaged")]
    protected readonly List<IDamageable> damageableObjects = new List<IDamageable>();
    
    private int _parryHash = Animator.StringToHash("parry");
    private int _breakHash = Animator.StringToHash("Break");

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
        
        damageableObjects.Add(damageTarget);
        
        WorldCharacterEffectsManager.Instance.OnParrySuccess(contactPoint);
        damageTarget.characterVariableManager.SuccessParry(ownerCharacter); // 무적효과 
        damageTarget.characterAnimatorManager.PlayTargetActionAnimation(_parryHash, true);
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
        if (damageTarget.characterStatsManager.UseStamina()) return true;
        // 없음 -> 블록 실패 
        damageTarget.characterAnimatorManager.PlayTargetActionAnimation(_breakHash, true);
        return false;
    }

    protected void DamageTarget(IDamageable damageTarget, float physicalDamage, float magicalDamage,  bool isBlock = false)
    {
        if (damageableObjects.Contains(damageTarget))
            return;
        
        damageableObjects.Add(damageTarget);

        TakeDamageEffect damageEffect = WorldCharacterEffectsManager.Instance.takeDamageEffect;
        
        MonoBehaviour monoBehaviourTarget = damageTarget as MonoBehaviour;
        if(monoBehaviourTarget)
            damageEffect.SetDamage(
                physicalDamage, 
                magicalDamage, 
                ownerCharacter.characterStatsManager.extraDamage.Value, 
                contactPoint,
                Vector3.SignedAngle(ownerCharacter.transform.forward, monoBehaviourTarget.transform.forward, Vector3.up),
                isBlock
                );
        
        Debug.LogWarning("DAMAGE INFO :" + damageEffect);
        
        ModifyDamageEffect(damageEffect);
        damageTarget.ProcessInstantEffect(damageEffect);
    }

    protected virtual void ModifyDamageEffect(TakeDamageEffect damageEffect) { }
}