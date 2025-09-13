using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageData damageData);
    bool IsAlive { get; }
}

public struct DamageData
{
    public CharacterManager attacker;
    public float physicalDamage;
    public float magicalDamage;
    public float extraDamage;
    public float poiseDamage;
    public Vector3 contactPoint;
    public float angleHitFrom;
}
