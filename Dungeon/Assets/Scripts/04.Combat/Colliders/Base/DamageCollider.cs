using UnityEngine;

public class DamageCollider : DamageLogic
{
    protected Collider[] damageColliders;
    
    protected float physicalDamage;
    protected float magicalDamage;

    protected override void Awake()
    {
        base.Awake();
        damageColliders = GetComponentsInChildren<Collider>();
    }
    
    protected virtual void Start()
    {
        foreach (var damageCollider in damageColliders)
        {
            damageCollider.enabled = false;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();
        if(!damageTarget) return;
        contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        
        if(ownerCharacter.characterGroup == damageTarget.characterGroup) return;
        
        SetBlockingDotValues(damageTarget);
        if(CheckForParried(damageTarget)) return;
        
        DamageTarget(damageTarget, physicalDamage, magicalDamage, CheckForBlock(damageTarget));
    }

    #region Public Method

    public void SetWeaponDamage(CharacterManager owner, EquipmentItemInfoWeapon equipmentItemInfoWeapon)
    {
        ownerCharacter = owner;
        physicalDamage = equipmentItemInfoWeapon.physicalDamage * owner.characterVariableManager.physicalDamageMultiplier.Value;
        magicalDamage = equipmentItemInfoWeapon.magicalDamage * owner.characterVariableManager.magicalDamageMultiplier.Value;
    }

    #region AnimationEvent
    public virtual void EnableDamageCollider()
    {
        damageableObjects.Clear();
        foreach (var damageCollider in damageColliders)
        {
            damageCollider.enabled = true;
        }
    }

    public virtual void DisableDamageCollider()
    {
        foreach (var damageCollider in damageColliders)
        {
            damageCollider.enabled = false;
        }
        damageableObjects.Clear();
    }
    #endregion

    #endregion
    
    
}