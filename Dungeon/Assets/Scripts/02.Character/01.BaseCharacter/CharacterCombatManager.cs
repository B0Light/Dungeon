using System.Collections;
using UnityEngine;


public enum AttackType
{
    LightAttack,
    HeavyAttack,
    Block,
    Skill,
}

public class CharacterCombatManager : MonoBehaviour
{
    protected CharacterManager character;
    
    [HideInInspector]  public int lastAttackAction;
    [HideInInspector]  public CharacterManager currentTarget;
    [HideInInspector]  public AttackType currentAttackType;

    // Lock On
    [HideInInspector]  public Transform lockOnTransform;

    [HideInInspector]  public CharacterManager criticalDamagedCharacter;
    [HideInInspector]  public bool canCriticalAttack = false;
    
    // Action Flag
    [HideInInspector] public bool enableCanDoCombo = false;
    [HideInInspector] public bool canPerformRollingAttack = false;
    [HideInInspector] public bool canPerformBackStepAttack = false;
    [HideInInspector] public bool canPerformJumpingAttack = false;
    
    // WeaponAction
    public BaseAttackAction_Light lightAttackAction;
    public BaseAttackAction_Heavy heavyAttackAction;
    public BaseAttackAction_Skill skillAction;
    public BlockAction blockAction;
    
    private const float ChallengeDistance = 2.0f;

    private Dir _curDodgeDir;

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }
    
    public void ChallengeTarget()
    {
        CharacterManager target = character.CurrentTarget;
        if(target == null) return;
        target.transform.LookAt(transform);
        Vector3 targetForward = target.transform.forward;
        Vector3 desiredPosition = target.transform.position + targetForward * ChallengeDistance;
        desiredPosition.y = target.transform.position.y;
        transform.position = desiredPosition;
        character.characterVariableManager.CLVM.isCrouching = false;
        target.characterVariableManager.CLVM.isCrouching = false;
        target.characterLocomotionManager.StopLocomotion();
        character.isBattle.Value = true;
        character.CurrentTarget.isBattle.Value = true;
        Debug.Log("On Battle");
    }

    public void Attack(Dir dir)
    {
        character.characterAnimatorManager.PlayDirAttackAnimation(dir);
    }

    public void Dodge(float dir)
    {
        // 공격 방향 == 회피방향 : 회피
        _curDodgeDir = dir > 0 ? Dir.Right : Dir.Left;
        character.characterAnimatorManager.PlayDodgeAnimation(_curDodgeDir);
    }
    
    public void ReactToSound(CharacterManager source)
    {
        currentTarget = source;
    }
    
    #region public Animation Method
    public void EnableIsInvulnerable()
    {
        character.characterVariableManager.isInvulnerable.Value = true;
    }

    public void DisableIsInvulnerable()
    {
        character.characterVariableManager.isInvulnerable.Value = false;
    }
    
    public void EnableCanDoCombo()
    {
        enableCanDoCombo = true;
    }

    public void DisableCanDoCombo()
    {
        enableCanDoCombo = false;
    }
    
    public void EnableRollingAttack()
    {
        canPerformRollingAttack = true;
    }
    
    public void DisableRollingAttack()
    {
        canPerformRollingAttack = false;
    }
    
    public void EnableBackStepAttack()
    {
        canPerformBackStepAttack = true;
    }
    
    public void DisableBackStepAttack()
    {
        canPerformBackStepAttack = false;
    }
    
    public void EnableJumpingAttack()
    {
        Debug.Log("Can Jump Attack");
        canPerformJumpingAttack = true;
    }
    
    public void DisableJumpingAttack()
    {
        canPerformJumpingAttack = false;
    }

    #endregion
    
}