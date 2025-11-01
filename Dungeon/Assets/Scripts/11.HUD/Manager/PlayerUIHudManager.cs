using UnityEngine;

public class PlayerUIHudManager : HUDComponent
{
    [HideInInspector] public PlayerUIStatusManager playerUIStatusManager;
    [HideInInspector] public PlayerUIWeaponSlotManager playerUIWeaponSlotManager;

    protected override void Awake()
    {
        base.Awake();
        playerUIStatusManager = GetComponentInChildren<PlayerUIStatusManager>();
        playerUIWeaponSlotManager = GetComponentInChildren<PlayerUIWeaponSlotManager>();
    }
}

