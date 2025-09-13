using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Test Action")]
public class WeaponItemAction : ScriptableObject
{
    public int actionID;
    public int actionCost = 1;
    [SerializeField] private int hungerCost = 1;

    public virtual void AttemptToPerformAction(PlayerManager playerPerformingAction, EquipmentItemInfoWeapon usedWeaponItemInfo)
    {
        if (playerPerformingAction && usedWeaponItemInfo)
        {
            // 공격키 연타시 배고픔 계속 감소하는 버그 제거 
            if(!playerPerformingAction.isPerformingAction)
                playerPerformingAction.playerVariableManager.hungerLevel.Value -= hungerCost;
        }
        else
        {
            Debug.Log("No Performing Action!!");
        }
    }
}
