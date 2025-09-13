using System;
using UnityEngine;

[Serializable]
public class ProjectileConfiguration
{
    [Header("Basic Settings")]
    public ProjectileType projectileType;
    public GameObject projectilePrefab;
    public ProjectileBehavior behavior = ProjectileBehavior.Physical;
    public int initialPoolSize = 10;

    [Header("Physical Projectile Settings")]
    public float projectileSpeed = 20f;
    public float maxRange = 100f;
    public LayerMask collisionMask = -1;
    [Header("Area Effect")]
    public float areaOfEffect = 0f; // 범위 공격 반지름 (0이면 단일 타겟)
    [Header("Static Effect")]
    public bool isStaticByDefault = false; // 기본적으로 정적 이펙트인지
    public float staticDuration = 2f; // 정적 이펙트 지속 시간

    [Header("Damage Settings")]
    [Range(0f, 1f)] public float physicalDamageRatio = 1f; // 물리 데미지 비율
    [Range(0f, 1f)] public float magicalDamageRatio = 0f;  // 마법 데미지 비율
    public float damage = 10f;           // 기본 데미지
    public float extraDamage = 0f;       // 추가 데미지
    public float poiseDamage = 5f;       // 포이즈 데미지
    public float damageModifier = 1f;    // 데미지 배율 (크리티컬 등)
    
    [Header("Penetration Settings")]
    public bool piercing = false;
    public int maxPierceCount = 1;

    [Header("Status Effects")]
    public bool applyStatusEffect = false;
    public StatusEffectType statusEffectType = StatusEffectType.None;
    public float statusEffectDuration = 3f;
    public float statusEffectIntensity = 1f;
    
    [Header("Impact Effects")]
    public EffectPlayer impactParticlePrefab; // 충돌 시 생성할 파티클 프리팹
    public EffectPlayer effectObjectPrefab; // 일반 이펙트 생성용 프리팹
    
    

    private void OnValidate()
    {
        // 물리 + 마법 데미지 비율이 1을 넘지 않도록 제한
        if (physicalDamageRatio + magicalDamageRatio > 1f)
        {
            float total = physicalDamageRatio + magicalDamageRatio;
            physicalDamageRatio /= total;
            magicalDamageRatio /= total;
        }
    }
}