using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageData damageData);
    bool IsAlive { get; }
}

public struct DamageData
{
    public float physicalDamage;
    public float magicalDamage;
    public float extraDamage;
    public Vector3 contactPoint;
    public float angleHitFrom;
}
