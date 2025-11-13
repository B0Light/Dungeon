using UnityEngine;


public class CharacterAnimatorManager : MonoBehaviour
{
    protected CharacterManager characterManager;

    [Header("Flags")]
    public bool applyRootMotion = false;

    public int lastDamageAnimationPlayed;
    
    private readonly int _dead = Animator.StringToHash("Dead");

    public readonly int hitForward  = Animator.StringToHash("hit_Forward_Medium_01");
    public readonly int hitBackward = Animator.StringToHash("hit_Backward_Medium_01");
    public readonly int hitLeft     = Animator.StringToHash("hit_Left_Medium_01");
    public readonly int hitRight    = Animator.StringToHash("hit_Right_Medium_01");

    public readonly int blockForward  = Animator.StringToHash("A_Blocking_F_Sword");
    public readonly int blockLeft     = Animator.StringToHash("A_Blocking_L_Sword");
    public readonly int blockRight    = Animator.StringToHash("A_Blocking_R_Sword");
    public readonly int groggy        = Animator.StringToHash("Groggy");
    
    public readonly int criticalFrontVictim = Animator.StringToHash("criticalAttack_Front_Victim");
    public readonly int criticalBackVictim  = Animator.StringToHash("criticalAttack_Back_Victim");

    public void Spawn()
    {
        characterManager = GetComponent<CharacterManager>();
    }
    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    public void PlayTargetActionAnimation(
        int targetAnimation,
        bool isPerformingAction = true, 
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if (characterManager.isDead.Value || characterManager.animator == null) return;
        
        applyRootMotion = rootMotion;
        characterManager.isPerformingAction = isPerformingAction;
        characterManager.characterLocomotionManager.canRotate = canRotate;
        characterManager.characterLocomotionManager.canMove = canMove;
        characterManager.animator.CrossFade(targetAnimation, 0.2f);
    }
    
    public void PlayTargetAttackActionAnimation(
        int targetAnimation,
        float actionPoint = 1f,
        bool isPerformingAction = true,
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        if(!characterManager.isPerformingAction)
            if(!characterManager.characterStatsManager.UseStamina(actionPoint)) return;
        characterManager.characterCombatManager.lastAttackAction = targetAnimation;
        PlayTargetActionAnimation(targetAnimation, isPerformingAction, rootMotion, canRotate, canMove);
    }
    
    public void PlayDeadAnimation()
    {
        PlayTargetActionAnimation(_dead);
    }

    public void UpdateAnimatorController(AnimatorOverrideController weaponController)
    {
        characterManager.animator.runtimeAnimatorController = weaponController;
    }
}

