using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;


public class CharacterAnimatorManager : MonoBehaviour
{
    protected CharacterManager characterManager;

    [Header("Flags")]
    public bool applyRootMotion = false;

    [Header("Damaged Animation")]
    public string lastDamageAnimationPlayed;

    public string hitForward  = "hit_Forward_Medium_01";
    public string hitBackward = "hit_Backward_Medium_01";
    public string hitLeft     = "hit_Left_Medium_01";
    public string hitRight    = "hit_Right_Medium_01";

    [ReadOnly] public readonly string blockForward  = "A_Blocking_F_Sword";
    [ReadOnly] public readonly string blockLeft     = "A_Blocking_L_Sword";
    [ReadOnly] public readonly string blockRight    = "A_Blocking_R_Sword";
    [ReadOnly] public readonly string groggy        = "Groggy";

    [Header("Critical Effect")]
    [ReadOnly] public readonly string criticalAttack_Back = "CriticalAttack_Back";
    [ReadOnly] public readonly string criticalAttack_Front = "CriticalAttack_Front";
    [ReadOnly] public readonly string criticalAttack_Back_Victim = "CriticalAttack_Back_Victim";
    [ReadOnly] public readonly string criticalAttack_Front_Victim = "CriticalAttack_Front_Victim";
    [ReadOnly] public readonly string criticalAttack_Back_Death = "CriticalAttack_Back_Death";
    [ReadOnly] public readonly string criticalAttack_Front_Death = "CriticalAttack_Front_Death";
    public void Spawn()
    {
        characterManager = GetComponent<CharacterManager>();
    }
    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public void PlayTargetActionAnimation(
        string targetAnimation,
        bool isPerformingAction, 
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (targetAnimation != "Dead_01" && characterManager.isDead.Value) return;
        
        applyRootMotion = rootMotion;
        characterManager.animator.CrossFade(targetAnimation, 0.2f);
        characterManager.isPerformingAction = isPerformingAction;
        characterManager.characterLocomotionManager.canRotate = canRotate;
        characterManager.characterLocomotionManager.canMove = canMove;
    }

    public void PlayTargetAttackActionAnimation(
        EquipmentItemInfoWeapon equipmentItemInfoWeapon,
        AttackType attackType,
        string targetAnimation,
        bool isPerformingAction,
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false,
        int actionPoint = 1)
    {
        if (targetAnimation != "Dead_01" && characterManager.isDead.Value) return;
        
        characterManager.characterStatsManager.UseActionPoint(actionPoint);
        characterManager.characterCombatManager.currentAttackType = attackType;
        characterManager.characterCombatManager.lastAttackAnimationPerformed = targetAnimation;
        UpdateAnimatorController(equipmentItemInfoWeapon.weaponAnimator);
        applyRootMotion = rootMotion;
        characterManager.isPerformingAction = isPerformingAction;
        characterManager.characterLocomotionManager.canRotate = canRotate;
        characterManager.characterLocomotionManager.canMove = canMove;
        characterManager.animator.CrossFade(targetAnimation, 0.2f);
    }

    public void UpdateAnimatorController(AnimatorOverrideController weaponController)
    {
        characterManager.animator.runtimeAnimatorController = weaponController;
    }
}

