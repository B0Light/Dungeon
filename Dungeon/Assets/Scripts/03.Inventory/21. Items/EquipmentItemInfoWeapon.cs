using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * physical Damage : ENEMY
 *
 * Slash / Stab : player Attack
 */

public class EquipmentItemInfoWeapon : EquipmentItemInfo
{
    [Header("Animations")]
    [HideInInspector] public AnimatorOverrideController weaponAnimator;
    
    [Header("Weapon Equip Sprite")]
    [HideInInspector] public Sprite weaponEquipSprite;
    
    [Header("Weapon Base Damage")]
    [HideInInspector] public int physicalDamage = 0;
    [HideInInspector] public int magicalDamage = 0;

    [Header("Weapon Base Poise Damage")] 
    public float poiseDamage = 10;
    [Header("Attack Speed")] 
    public float attackSpeed = 1;

    [HideInInspector] public float hAtkMod01 = 1.0f;
    [HideInInspector] public float hAtkMod02 = 1.2f;
    [HideInInspector] public float hAtkMod03 = 2.0f;
    [HideInInspector] public float vAtkMod01 = 1.4f;
    [HideInInspector] public float vAtkMod02 = 1.6f;
    [HideInInspector] public float vAtkMod03 = 2.0f;
    [HideInInspector] public float runningAtkMod = 1.1f;
    [HideInInspector] public float rollingAtkMod = 2.0f;
    [HideInInspector] public float backStepAtkMod = 1.5f;
    [HideInInspector] public float jumpingAtkMod = 5.0f;
    
    [Header("Stamina Costs Modifiers")] 
    [HideInInspector] public int baseActionCost = 1;

    [Header("Weapon Blocking Absorption")] 
    [HideInInspector] public int physicalDamageAbsorption = 50;
    [HideInInspector] public int magicalDamageAbsorption = 50;
    [HideInInspector] public int stability = 50; // 가드시 poiseDmg 가드 정도 
    
    [Header("Actions")] 
    public WeaponItemAction lightAttackAction; 
    public WeaponItemAction heavyAttackAction;
    public WeaponItemAction blockAction;
    public WeaponItemAction weaponSkill;
}
