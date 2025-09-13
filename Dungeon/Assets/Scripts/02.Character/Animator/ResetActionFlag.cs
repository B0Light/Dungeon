using UnityEngine;

public class ResetActionFlag : StateMachineBehaviour
{
    private CharacterManager _character;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_character == null)
        {
            _character = animator.GetComponent<CharacterManager>();
        }

        // THIS IS CALLED WHEN AN ACTION ENDS, AND THE STATE RETURNS TO "EMPTY"
        _character.isPerformingAction = false;
        _character.characterAnimatorManager.applyRootMotion = true;
        _character.characterLocomotionManager.canRotate = true;
        _character.characterLocomotionManager.canMove = true;
        //character.characterLocomotionManager.isRolling = false;
        
        _character.characterEquipmentManager.CloseDamageCollider();
        
        _character.characterCombatManager.DisableCanDoCombo();
        _character.characterCombatManager.DisableCanDoRollingAttack();
        _character.characterCombatManager.DisableCanDoBeckStepAttack();
        _character.characterCombatManager.DisableCanDoJumpingAttack();
        
        _character.characterVariableManager.isInvulnerable.Value = false;
        _character.characterVariableManager.isAttacking.Value = false;
        _character.characterVariableManager.isCharging.Value = false;
        _character.characterVariableManager.isBlock.Value = false;
        _character.characterVariableManager.isParring.Value = false;
        _character.characterVariableManager.isTrailActive.Value = false;

        
        
        //character.characterLocomotionManager.SetLocomotionState();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}

