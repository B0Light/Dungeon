using UnityEngine;

public class ResetDodgeAction :StateMachineBehaviour
{
    private CharacterManager _character;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_character == null)
        {
            _character = animator.GetComponent<CharacterManager>();
        }
        
        _character.characterLocomotionManager.SetLocomotionState();
    }
}
