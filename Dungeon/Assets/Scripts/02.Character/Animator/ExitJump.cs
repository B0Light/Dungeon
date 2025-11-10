using UnityEngine;

public class ExitJumpState : StateMachineBehaviour
{
    CharacterManager character;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(character == null)
        {
            character = animator.GetComponent<CharacterManager>();
        }

        character.characterCombatManager.DisableJumpingAttack();
    }
}
