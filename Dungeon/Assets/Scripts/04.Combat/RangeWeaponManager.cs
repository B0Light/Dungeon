using UnityEngine;

public class RangeWeaponManager : MonoBehaviour, IWeaponManager
{
    private EquipmentItemInfoWeapon _weaponInfo;
    private CharacterManager _owner;
    
    private UnifiedProjectilePoolManager _poolManager;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ProjectileType projectileType = 0;
    
    private void Start()
    {
        _poolManager = FindAnyObjectByType<UnifiedProjectilePoolManager>();
        if (firePoint == null) firePoint = transform;
    }
    
    public void SetWeapon(CharacterManager characterWieldingWeapon ,EquipmentItemInfoWeapon equipmentItemInfoWeapon)
    {
        _weaponInfo = equipmentItemInfoWeapon;
        _owner = characterWieldingWeapon;
    }
    
    public void OpenDamageCollider()
    {
        GUIController.Instance.playerUIHudManager.playerUIWeaponSlotManager.GlowWeaponSlot();
        
    }
    
    public void CloseDamageCollider()
    {
        
    }

    private void FireProjectile()
    {
        _poolManager.FireInDirection(
            null, 
            projectileType,
            firePoint.position,
            firePoint.forward,
            firePoint
        );
        
        //Transform target = FindObjectOfType<IDamageable>().transform;
        // projectilePoolManager.FireAtTarget(projectileType, firePoint.position, target, firePoint);
    }
}
