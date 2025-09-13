using UnityEngine;

public class QuadrupedAILocomotionManager : AICharacterLocomotionManager
{
    private QuadrupedAIManager _quadrupedAIManager;
    
    private readonly int _movementHash = Animator.StringToHash("Movement_f");
    private readonly int _blinkTriggerHash = Animator.StringToHash("Blink_tr");
    private readonly int _jumpTriggerHash = Animator.StringToHash("Jump_tr");
    protected override void Awake()
    {
        base.Awake();
        _quadrupedAIManager = characterManager as QuadrupedAIManager;
    }

    protected override void UpdateAnimatorController()
    {
        characterManager.animator.SetTrigger(_blinkTriggerHash);

        float speedValue = CLVM.speed2D / CLVM.sprintSpeed;
        
        speedValue = speedValue < 0.25f ? 0f : speedValue < 0.5f ? 0.5f : 1f;
        
        characterManager.animator.SetFloat(_movementHash, speedValue);
    }

    protected override void EnterJumpState()
    {
        characterManager.animator.SetTrigger(_jumpTriggerHash);

        CLVM.isSliding = false;

        CLVM.velocity = new Vector3(CLVM.velocity.x, CLVM.jumpForce, CLVM.velocity.z);
        CLVM.jumpForce = CLVM.originJumpForce;
    }
}
