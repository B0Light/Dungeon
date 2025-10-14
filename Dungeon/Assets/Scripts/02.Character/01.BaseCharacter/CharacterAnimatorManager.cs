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
        int targetAnimation,
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false
        )
    {
        if (targetAnimation != Animator.StringToHash("Dead_01") && characterManager.isDead.Value) return;
        
        UpdateAnimatorController(equipmentItemInfoWeapon.weaponAnimator);
        applyRootMotion = rootMotion;
        characterManager.isPerformingAction = true;
        characterManager.characterLocomotionManager.canRotate = canRotate;
        characterManager.characterLocomotionManager.canMove = canMove;
        characterManager.animator.CrossFade(targetAnimation, 0.2f);
    }

    public void UpdateAnimatorController(AnimatorOverrideController weaponController)
    {
        characterManager.animator.runtimeAnimatorController = weaponController;
    }
}

