using UnityEngine;


public enum AttackType
{
    LightAttack01,
    LightAttack02,
    LightAttack03,
    HeavyAttack01,
    HeavyAttack02,
    HeavyAttack03,
    ChargeAttack01,
    ChargeAttack02,
    ChargeAttack03,
    Parry,
    Block,
    RunningAttack01,
    RollingAttack01,
    BackStepAttack01,
    JumpingAttack01,
    CriticalAttack,
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

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }
    
    #region public Method
    
    public virtual void SetTarget(CharacterManager newTarget)
    {
        if(currentTarget == newTarget) return;
        
        if(newTarget != null)
        {
            currentTarget = newTarget;
            character.characterVariableManager.CLVM.isSprinting = true;
            Debug.Log("SET TARGET : " + newTarget.name);
        }
        else
        {
            currentTarget = null;
            character.characterVariableManager.CLVM.isSprinting = false;
            Debug.Log("RESET TARGET");
        }
    }

    public void ReactToSound(CharacterManager source)
    {
        currentTarget = source;
    }
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