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
    #region Animation Hash
    private readonly int _dead = Animator.StringToHash("Dead");
    // Hit Action
    private readonly int hitForward  = Animator.StringToHash("hit_Forward_Medium_01");
    private readonly int hitBackward = Animator.StringToHash("hit_Backward_Medium_01");
    private readonly int hitLeft     = Animator.StringToHash("hit_Left_Medium_01");
    private readonly int hitRight    = Animator.StringToHash("hit_Right_Medium_01");
    // Block Action
    private readonly int blockForward  = Animator.StringToHash("A_Blocking_F_Sword");
    private readonly int blockLeft     = Animator.StringToHash("A_Blocking_L_Sword");
    private readonly int blockRight    = Animator.StringToHash("A_Blocking_R_Sword");
    // AttackAction
    private readonly int _attackUp = Animator.StringToHash("Attack_Up");
    private readonly int _attackUpRight = Animator.StringToHash("Attack_Up_Right");
    private readonly int _attackRight = Animator.StringToHash("Attack_Right");
    private readonly int _attackDownRight = Animator.StringToHash("Attack_Down_Right");
    private readonly int _attackDown = Animator.StringToHash("Attack_Down");
    private readonly int _attackDownLeft = Animator.StringToHash("Attack_Down_Left");
    private readonly int _attackLeft = Animator.StringToHash("Attack_Left");
    private readonly int _attackUpLeft = Animator.StringToHash("Attack_Up_Left");

    private readonly int _dodgeLeft = Animator.StringToHash("Dodge_L");
    private readonly int _dodgeRight = Animator.StringToHash("Dodge_R");
    
    // ETC
    public readonly int groggy = Animator.StringToHash("Groggy");
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
        characterManager.animator.CrossFade(targetAnimation, 0f);
    }
    
    public void PlayTargetAttackActionAnimation(
        int targetAnimation,
        float staminaValue = 1f,
        bool isPerformingAction = true,
        bool rootMotion = true,
        bool canRotate = false,
        bool canMove = false)
    {
        characterManager.characterStatsManager.UseStamina(staminaValue);
        characterManager.characterCombatManager.lastAttackAction = targetAnimation;
        PlayTargetActionAnimation(targetAnimation, isPerformingAction, rootMotion, canRotate, canMove);
    }

    #region Dir Attack Version

    public void PlayDirAttackAnimation(Dir dir)
    {
        int targetAnimation = 0;
        switch (dir)
        {
            case Dir.Up:
                targetAnimation = _attackUp;
                break;
            case Dir.UpRight:
                targetAnimation = _attackUpRight;
                break;
            case Dir.Right:
                targetAnimation = _attackRight;
                break;
            case Dir.DownRight:
                targetAnimation = _attackDownRight;
                break;
            case Dir.Down:
                targetAnimation = _attackDown;
                break;
            case Dir.DownLeft:
                targetAnimation = _attackDownLeft;
                break;
            case Dir.Left:
                targetAnimation = _attackLeft;
                break;
            case Dir.UpLeft:
                targetAnimation = _attackUpLeft;
                break;
            default:
                targetAnimation = _attackDown;
                break;
            
        }
        PlayTargetAttackActionAnimation(targetAnimation, 0, true, false, false, false);
    }
    
    public void PlayDirectionalHitAnimation(float angle, bool isBlock)
    {
        int animationValue = 0;
        // 각도에 따른 피격 방향 결정
        if ((angle >= 145 && angle <= 180) || (angle >= -180 && angle <= -145))
            animationValue =  isBlock ? blockForward : hitForward;
        
        if (angle >= -45 && angle <= 45) // 뒤에서 떄리는 건 가드 불가 
            animationValue =  hitBackward;
        
        if (angle >= -144 && angle <= -46)
            animationValue =  isBlock ? blockLeft : hitLeft;
        
        if (angle >= 46 && angle <= 144)
            animationValue =  isBlock ? blockRight : hitRight;

        PlayTargetActionAnimation(animationValue);
    }

    public void PlayDodgeAnimation(Dir dir)
    {
        var dodgeDir = dir == Dir.Left ? _dodgeLeft : _dodgeRight;
        PlayTargetActionAnimation(dodgeDir);
    }

    #endregion
    
    public void PlayDeadAnimation()
    {
        PlayTargetActionAnimation(_dead);
    }

    public void UpdateAnimatorController(AnimatorOverrideController weaponController)
    {
        characterManager.animator.runtimeAnimatorController = weaponController;
    }
}

