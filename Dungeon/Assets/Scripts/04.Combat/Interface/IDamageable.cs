using UnityEngine;

public interface IDamageable : IEffectable
{
    void ProcessInstantEffect(TakeDamageEffect damageEffect);
    void TakeDamage(float finalDamage, float poiseDamage);
    void PostDamageEffect(Vector3 contactPoint, float angleHitFrom, bool isBlock);
    
    bool IsOpponent(WorldUtilityManager.CharacterGroup characterGroup);
    bool CanTakeDamage();

    float GetPhysicalAbsorption(bool isBlock);
    float GetMagicalAbsorption(bool isBlock);

}

public struct DamageData
{
    public float physicalDamage;
    public float magicalDamage;
    public float extraDamage;
    public Vector3 contactPoint;
    public float angleHitFrom;
}
