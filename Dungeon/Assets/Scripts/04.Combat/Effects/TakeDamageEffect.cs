using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Damage")]
public class TakeDamageEffect : IInstantEffect
{
    #region Variables
    
    private float _physicalDamage;
    private float _magicalDamage;
    private float _extraDamage;
    
    private float _finalDamageDealt;
    private float _finalPoiseDamage;
    
    private Vector3 _contactPoint; 
    private float _angleHitFrom;

    private bool _isBlock;
    
    #endregion

    #region Public Methods
    
    public void SetDamage(float physicalDmg, float magicalDmg, float extraDmg, Vector3 contact, float angle, bool isBlock)
    {
        _physicalDamage = physicalDmg;
        _magicalDamage = magicalDmg;
        _extraDamage = extraDmg;
        _contactPoint = contact;
        _angleHitFrom = angle;
        _isBlock = isBlock;
    }

    public void ApplyAttackDamageModifiers(float modifier)
    {
        _physicalDamage *= modifier;
        _magicalDamage *= modifier;
    }

    public override void ProcessEffect(IEffectable effectTarget)
    {
        if (effectTarget is IDamageable damageable)
        {
            if (!damageable.CanTakeDamage()) return;

            CalculateDamage(damageable);
            ApplyDamage(damageable);
            HandlePostHitEffects(damageable);
        }
    }
    
    #endregion

    #region Protected Methods
    
    private void CalculateDamage(IDamageable hitTarget)
    {
        // 방어력 적용 계산
        var physicalAbsorption = hitTarget.GetPhysicalAbsorption(_isBlock);
        var magicalAbsorption = hitTarget.GetMagicalAbsorption(_isBlock);
        
        // 데미지 계산
        var reducedPhysicalDamage = _physicalDamage * (100 - physicalAbsorption) / 100;
        var reducedMagicalDamage = _magicalDamage * (100 - magicalAbsorption) / 100;
        
        // 최종 데미지 및 포이즈 데미지 계산
        _finalDamageDealt = Mathf.RoundToInt(reducedPhysicalDamage + reducedMagicalDamage + _extraDamage);
        
        // 최소 데미지 보장
        if(_finalDamageDealt <= 0)
        {
            _finalDamageDealt = 1;
        }
        
        //Debug.LogWarning($"[DamageINFO] B : {physicalAbsorption} D : {magicalAbsorption} / A : {physicalDamage} C : {magicalDamage} / V : {finalDamageDealt}" );
    }

    private void ApplyDamage(IDamageable hitTarget)
    {
        if(!hitTarget.CanTakeDamage()) return;
        
        hitTarget.TakeDamage(_finalDamageDealt, _finalPoiseDamage);
    }

    private void HandlePostHitEffects(IDamageable hitTarget)
    {
        hitTarget.PostDamageEffect(_contactPoint, _angleHitFrom, _isBlock);
    }
    
    #endregion
}