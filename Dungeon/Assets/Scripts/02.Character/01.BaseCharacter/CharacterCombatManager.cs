using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI.Extensions;

public class CharacterCombatManager : MonoBehaviour
{
    protected CharacterManager character;

    [Header("Last Attack Anim Performed")]
    [HideInInspector]  public string lastAttackAnimationPerformed;

    [Header("Attack Target")]
    [HideInInspector]  public CharacterManager currentTarget;

    [Header("Attack Type")]
    [HideInInspector]  public AttackType currentAttackType;

    [Header("Lock On Transform")]
    [HideInInspector]  public Transform lockOnTransform;

    [HideInInspector]  public CharacterManager criticalDamagedCharacter;
    [HideInInspector]  public bool canCriticalAttack = false;
        
    [Header("Attack Flag")]
    [HideInInspector]  public bool canPerformRollingAttack = false;
    [HideInInspector]  public bool canPerformBackStepAttack = false;
    [HideInInspector]  public bool canPerformJumpingAttack = false;
    
    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

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
    
    public virtual void EnableCanDoCombo()
    {
       
    }

    public virtual void DisableCanDoCombo()
    {

    }
    
    public void EnableCanDoRollingAttack()
    {
        canPerformRollingAttack = true;
    }
    
    public void DisableCanDoRollingAttack()
    {
        canPerformRollingAttack = false;
    }
    
    public void EnableCanDoBeckStepAttack()
    {
        canPerformBackStepAttack = true;
    }
    
    public void DisableCanDoBeckStepAttack()
    {
        canPerformBackStepAttack = false;
    }
    
    public void EnableCanDoJumpingAttack()
    {
        Debug.Log("Can Jump Attack");
        canPerformJumpingAttack = true;
    }
    
    public void DisableCanDoJumpingAttack()
    {
        canPerformJumpingAttack = false;
    }
}
