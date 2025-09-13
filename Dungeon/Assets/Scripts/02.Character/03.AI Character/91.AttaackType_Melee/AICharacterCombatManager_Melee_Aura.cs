using System;
using UnityEngine;
using UnityEngine.Serialization;

public class AICharacterCombatManager_Melee_Aura : AICharacterCombatManager_Melee
{
    [System.Serializable]
    public class AuraAttackData
    {
        public AuraVFXDamageCollider collider;
        [Range(0.1f, 5f)] public float damageModifier = 1.0f;
        [Range(0f, 10f)] public float poiseDamage = 20f;
        [Range(0f, 5f)] public float delayTime = 0f;
        [Range(0f, 5f)] public float returnTime = 0f;
        
        public bool IsValid => collider != null;
    }

    [SerializeField] private AuraAttackData[] auraAttacks = new AuraAttackData[2]
    {
        new AuraAttackData { damageModifier = 1.1f, poiseDamage = 20f, delayTime = 0f, returnTime = 3f},
        new AuraAttackData { damageModifier = 1.3f, poiseDamage = 20f, delayTime = 0f, returnTime = 3f }
    };

    [SerializeField] private ProjectileType projectileType;

    private void Start()
    {
        InitializeAuraAttacks();
    }

    public override void OpenLeftHandDamageCollider()
    {
        base.OpenLeftHandDamageCollider();
    }

    public override void OpenRightHandDamageCollider()
    {
        base.OpenRightHandDamageCollider();
    }

    private void InitializeAuraAttacks()
    {
        for (int i = 0; i < auraAttacks.Length; i++)
        {
            if (auraAttacks[i].IsValid)
            {
                SetupAuraDamage(auraAttacks[i]);
            }
        }
    }

    private void SetupAuraDamage(AuraAttackData auraData)
    {
        auraData.collider.physicalDamage = baseDamage * auraData.damageModifier;
        auraData.collider.poiseDamage = auraData.poiseDamage;
    }

    // ANIMATIONS
    public void ExecuteAuraAttack(int auraIndex)
    {
        if (!IsValidAuraIndex(auraIndex)) return;
        
        var auraData = auraAttacks[auraIndex];
        if (!auraData.IsValid) return;

        auraData.collider.ownerCharacter = character;
        auraData.collider.EnableDamageColliderAfterDelay(auraData.delayTime, auraData.returnTime);
    }

    

    private bool IsValidAuraIndex(int index)
    {
        return index >= 0 && index < auraAttacks.Length;
    }
    
    public void MeleeAttack_Ground()
    {
        UnifiedProjectilePoolManager.Instance.FireInDirection(
            aiCharacter,
            projectileType, 
            transform.position, 
            Vector3.zero,
            transform
        );
    }
    
}