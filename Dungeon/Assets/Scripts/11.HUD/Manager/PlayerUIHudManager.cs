using UnityEngine;

public class PlayerUIHudManager : HUDComponent
{
    [HideInInspector] public PlayerUIStatusManager playerUIStatusManager;
    [HideInInspector] public PlayerUIQuickSlotManager playerUIQuickSlotManager;
    [HideInInspector] public PlayerUIWeaponSlotManager playerUIWeaponSlotManager;

    protected override void Awake()
    {
        base.Awake();
        playerUIStatusManager = GetComponentInChildren<PlayerUIStatusManager>();
        playerUIQuickSlotManager = GetComponentInChildren<PlayerUIQuickSlotManager>();
        playerUIWeaponSlotManager = GetComponentInChildren<PlayerUIWeaponSlotManager>();
    }
}

