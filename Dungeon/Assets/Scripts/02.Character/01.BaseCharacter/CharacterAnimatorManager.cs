using UnityEngine;

public enum Dir
{
    Up,        // 0
    UpRight,   // 1
    Right,     // 2
    DownRight, // 3
    Down,      // 4
    DownLeft,  // 5
    Left,      // 6
    UpLeft,    // 7
}

public class CharacterAnimatorManager : MonoBehaviour
{
    protected CharacterManager characterManager;

    [Header("Flags")]
    public bool applyRootMotion = false;
    public int lastDamageAnimationPlayed;
    #region Animation Hash
    private readonly int _dead = Animator.StringToHash("Dead");
    // Hit Action
    public readonly int hitForward  = Animator.StringToHash("hit_Forward_Medium_01");
    public readonly int hitBackward = Animator.StringToHash("hit_Backward_Medium_01");
    public readonly int hitLeft     = Animator.StringToHash("hit_Left_Medium_01");
    public readonly int hitRight    = Animator.StringToHash("hit_Right_Medium_01");
    // Block Action
    public readonly int blockForward  = Animator.StringToHash("A_Blocking_F_Sword");
    public readonly int blockLeft     = Animator.StringToHash("A_Blocking_L_Sword");
    public readonly int blockRight    = Animator.StringToHash("A_Blocking_R_Sword");
    // AttackAction
    public readonly int AttackUp = Animator.StringToHash("Attack_Up");
    public readonly int AttackUpRight = Animator.StringToHash("Attack_Up_Right");
    public readonly int AttackRight = Animator.StringToHash("Attack_Right");
    public readonly int AttackDownRight = Animator.StringToHash("Attack_Down_Right");
    public readonly int AttackDown = Animator.StringToHash("Attack_Down");
    public readonly int AttackDownLeft = Animator.StringToHash("Attack_Down_Left");
    public readonly int AttackLeft = Animator.StringToHash("Attack_Left");
    public readonly int AttackUpLeft = Animator.StringToHash("Attack_Up_Left");
    
    
    
    // ETC
    public readonly int groggy        = Animator.StringToHash("Groggy");
    public readonly int criticalFrontVictim = Animator.StringToHash("criticalAttack_Front_Victim");
    public readonly int criticalBackVictim  = Animator.StringToHash("criticalAttack_Back_Victim");
    
    

    #endregion
    
    
    

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
        if (characterManager.animator == null) return;
        if (characterManager.isDead.Value && targetAnimation != _dead) return;
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
        characterManager.characterCombatManager.lastAttackAction = targetAnimation;
        PlayTargetActionAnimation(targetAnimation, isPerformingAction, rootMotion, canRotate, canMove);
    }

    public void PlayDirAttackAnimation(Dir dir)
    {
        int targetAnimation = 0;
        switch (dir)
        {
            case Dir.Up:
                targetAnimation = AttackUp;
                break;
            case Dir.UpRight:
                targetAnimation = AttackUpRight;
                break;
            case Dir.Right:
                targetAnimation = AttackRight;
                break;
            case Dir.DownRight:
                targetAnimation = AttackDownRight;
                break;
            case Dir.Down:
                targetAnimation = AttackDown;
                break;
            case Dir.DownLeft:
                targetAnimation = AttackDownLeft;
                break;
            case Dir.Left:
                targetAnimation = AttackLeft;
                break;
            case Dir.UpLeft:
                targetAnimation = AttackUpLeft;
                break;
            default:
                targetAnimation = AttackDown;
                break;
            
        }
        PlayTargetAttackActionAnimation(targetAnimation, 0, true, false, false, false);
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

