using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Damage")]
public class TakeDamageEffect : IInstantCharacterEffect
{
    #region Variables
    
    protected CharacterManager attacker;
    protected float physicalDamage;
    protected float magicalDamage;
    protected float extraDamage;
    
    protected float finalDamageDealt;
    protected float finalPoiseDamage;

    protected float poiseDamage;     
    protected Vector3 contactPoint; 
    protected float angleHitFrom;
    
    #endregion

    #region Public Methods
    
    public void SetDamage(CharacterManager characterCausingDamage, float physicalDmg, float magicalDmg, 
                          float extraDmg, float poiseDmg, Vector3 contact, float angle)
    {
        attacker = characterCausingDamage;
        physicalDamage = physicalDmg;
        magicalDamage = magicalDmg;
        extraDamage = extraDmg;
        poiseDamage = poiseDmg;
        contactPoint = contact;
        angleHitFrom = angle;
    }

    public void ApplyAttackDamageModifiers(float modifier)
    {
        physicalDamage *= modifier;
        magicalDamage *= modifier;
        poiseDamage *= modifier;
    }

    public override void ProcessEffect(CharacterManager effectTarget)
    {
        // 무적 또는 죽은 상태면 데미지 처리 안함
        if (effectTarget.characterVariableManager.isInvulnerable.Value || effectTarget.isDead.Value) 
            return;

        CalculateDamage(effectTarget);
        ApplyDamage(effectTarget);
        HandlePostHitEffects(effectTarget);
    }
    
    #endregion

    #region Protected Methods
    
    protected virtual void CalculateDamage(CharacterManager hitTarget)
    {
        // 방어력 적용 계산
        float physicalAbsorption = hitTarget.characterStatsManager.basePhysicalAbsorption + 
                                  hitTarget.characterStatsManager.extraPhysicalAbsorption;
        float magicalAbsorption = hitTarget.characterStatsManager.baseMagicalAbsorption + 
                                 hitTarget.characterStatsManager.extraMagicalAbsorption;
        
        // 데미지 계산
        float reducedPhysicalDamage = physicalDamage * (100 - physicalAbsorption) / 100;
        float reducedMagicalDamage = magicalDamage * (100 - magicalAbsorption) / 100;
        
        // 최종 데미지 및 포이즈 데미지 계산
        finalDamageDealt = Mathf.RoundToInt(reducedPhysicalDamage + reducedMagicalDamage + extraDamage);
        finalPoiseDamage = poiseDamage * ((100 - hitTarget.characterStatsManager.passivePoise.Value) / 100);
        
        // 최소 데미지 보장
        if(finalDamageDealt <= 0)
        {
            finalDamageDealt = 1;
        }
        
        //Debug.LogWarning($"[DamageINFO] B : {physicalAbsorption} D : {magicalAbsorption} / A : {physicalDamage} C : {magicalDamage} / V : {finalDamageDealt}" );
    }

    protected virtual void ApplyDamage(CharacterManager hitTarget)
    {
        if(hitTarget.isDead.Value) return;
        
        //Debug.LogWarning("Final Dmg : " + finalDamageDealt);
        
        hitTarget.characterVariableManager.health.Value -= (int)finalDamageDealt;
        hitTarget.characterVariableManager.groggy.Value -= finalPoiseDamage;
    }

    protected virtual void HandlePostHitEffects(CharacterManager hitTarget)
    {
        PlayDirectionalBasedDamagedAnimation(hitTarget);
        PlayDamageSfx(hitTarget);
        PlayDamageVfx(hitTarget);
    }

    protected virtual void PlayDamageVfx(CharacterManager character)
    {
        character.characterEffectsManager.PlayBloodSplatterVFX(contactPoint);
    }

    protected virtual void PlayDamageSfx(CharacterManager character)
    {
        AudioClip physicalDamageSfx = WorldSoundFXManager.Instance.ChoosePhysicalDamageSfx();
        
        if (physicalDamageSfx != null)
            character.characterSoundFXManager.PlaySoundFX(physicalDamageSfx);

        character.characterSoundFXManager.PlayDamageGruntSoundFX();
    }

    protected virtual void PlayDirectionalBasedDamagedAnimation(CharacterManager character)
    {
        if (character.isDead.Value) return;

        string damageAnimation = GetDirectionalHitAnimation(character, angleHitFrom);
        if (string.IsNullOrEmpty(damageAnimation)) return;
        
        if (Random.Range(1, 10) >= 1) return;
        
        character.characterAnimatorManager.lastDamageAnimationPlayed = damageAnimation;
        character.characterAnimatorManager.PlayTargetActionAnimation(damageAnimation, true);
    }
    
    #endregion

    #region Private Methods
    
    private string GetDirectionalHitAnimation(CharacterManager character, float angle)
    {
        // 각도에 따른 피격 방향 결정
        if ((angle >= 145 && angle <= 180) || (angle >= -180 && angle <= -145))
            return character.characterAnimatorManager.hitForward;
        
        if (angle >= -45 && angle <= 45)
            return character.characterAnimatorManager.hitBackward;
        
        if (angle >= -144 && angle <= -46)
            return character.characterAnimatorManager.hitLeft;
        
        if (angle >= 46 && angle <= 144)
            return character.characterAnimatorManager.hitRight;
        
        return null;
    }
    
    #endregion
}