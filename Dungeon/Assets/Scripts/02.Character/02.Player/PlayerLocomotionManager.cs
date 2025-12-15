using UnityEngine;
public class PlayerLocomotionManager : CharacterLocomotionManager
{
    private PlayerManager _playerManager;
    
    private bool _canDoubleJump = false;
    private bool _canJumpLocomotion = false;
    private bool _canJumpCrouch = false;

    protected override void Start()
    {
        _playerManager = characterManager as PlayerManager;
        base.Start();
    }

    protected override void EnterState(AnimationState stateToEnter)
    {
        characterManager.characterVariableManager.CLVM.currentState = stateToEnter;
        switch (characterManager.characterVariableManager.CLVM.currentState)
        {
            case AnimationState.Base:
                EnterBaseState();
                break;
            case AnimationState.Locomotion:
                EnterLocomotionState();
                break;
            case AnimationState.Jump:
                EnterJumpState();
                break;
            case AnimationState.Fall:
                EnterFallState();
                break;
            case AnimationState.Crouch:
                EnterCrouchState();
                break;
            case AnimationState.DoubleJump:
                EnterDoubleJumpState();
                break;
        }
    }

    protected override void ExitCurrentState()
    {
        switch (characterManager.characterVariableManager.CLVM.currentState)
        {
            case AnimationState.Locomotion:
                ExitLocomotionState();
                break;
            case AnimationState.Jump:
            case AnimationState.DoubleJump:
                ExitJumpState();
                break;
            case AnimationState.Fall:
                ExitFallState();
                break;
            case AnimationState.Crouch:
                ExitCrouchState();
                break;
        }
    }
    
    protected override void UpdateAnimatorController()
    {
        base.UpdateAnimatorController();
        characterManager.animator.SetFloat(_cameraRotationOffsetHash, _cameraRotationOffset);
    }

    protected override void FaceMoveDirection()
    {
        if(!canRotate) return;
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(CLVM.moveDirection.x, 0f, CLVM.moveDirection.z).normalized;
        _cameraForward = PlayerCameraController.Instance.GetPlayerDir;
        
        Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

        CLVM.strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;

        CLVM.isTurningInPlace = false;
        
        if (CLVM.moveDirection.magnitude > 0.01)
        {
            if (_cameraForward != Vector3.zero)
            {
                CLVM.shuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                CLVM.shuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                UpdateStrafeDirection(
                    Vector3.Dot(characterForward, directionForward),
                    Vector3.Dot(characterRight, directionForward)
                );
                _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, CLVM.rotationSmoothing * Time.unscaledDeltaTime);

                float targetValue = CLVM.strafeAngle > CLVM.forwardStrafeMinThreshold && CLVM.strafeAngle < CLVM.forwardStrafeMaxThreshold ? 1f : 0f;

                if (Mathf.Abs(CLVM.forwardStrafe - targetValue) <= 0.001f)
                {
                    CLVM.forwardStrafe = targetValue;
                }
                else
                {
                    float t = Mathf.Clamp01(_STRAFE_DIRECTION_DAMP_TIME * Time.unscaledDeltaTime);
                    CLVM.forwardStrafe = Mathf.SmoothStep(CLVM.forwardStrafe, targetValue, t);
                }
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, CLVM.rotationSmoothing * Time.unscaledDeltaTime);
        }
        else
        {
            UpdateStrafeDirection(1f, 0f);

            float t = 20 * Time.unscaledDeltaTime;
            float newOffset = 0f;

            if (characterForward != _cameraForward)
            {
                newOffset = Vector3.SignedAngle(characterForward, _cameraForward, Vector3.up);
            }

            _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, newOffset, t);

            if (Mathf.Abs(_cameraRotationOffset) > 10)
            {
                CLVM.isTurningInPlace = true;
            }
        }
        
    }

    #region UseController

    protected override void CapsuleCrouchingSize(bool crouching)
    {
        if (crouching)
        {
            _playerManager.characterController.center = new Vector3(0f, CLVM.capsuleCrouchingCentre, 0f);
            _playerManager.characterController.height = CLVM.capsuleCrouchingHeight;
        }
        else
        {
            _playerManager.characterController.center = new Vector3(0f, CLVM.capsuleStandingCentre, 0f);
            _playerManager.characterController.height = CLVM.capsuleStandingHeight;
        }
    }

    protected override void Move()
    {
        if(!canLocomotion) return;
        
        float moveCoefficient = canMove ? 1 : 0;
        Vector3 moveValue = Time.unscaledDeltaTime * moveCoefficient * CLVM.velocity;
        _playerManager.characterController.Move(moveValue);
        
        if (CLVM.isLockedOn && CLVM.currentLockOnTarget != null)
        {
            CLVM.targetLockOnPos.position = CLVM.currentLockOnTarget.transform.position;
        }
    }

    protected override void GroundedCheck()
    {
        Vector3 curPos = _playerManager.characterController.transform.position;
        Vector3 spherePosition = new Vector3(
            curPos.x, curPos.y - CLVM.groundedOffset, curPos.z
        );
        CLVM.isGrounded = Physics.CheckSphere(spherePosition, _playerManager.characterController.radius, CLVM.groundLayerMask, QueryTriggerInteraction.Ignore);
        base.GroundedCheck();
    }

    #endregion

    #region State&Actions

    protected override void EnterLocomotionState()
    {
        _canJumpLocomotion = true;
    }
    
    protected override void ExitLocomotionState()
    {
        _canJumpLocomotion = false;
    }

    protected override void EnterCrouchState()
    {
        _canJumpCrouch = true;
    }
    
    protected override void ExitCrouchState()
    {
        _canJumpCrouch = false;
    }
    
    public void AttemptToRoll()
    {
        if(!CanPerformDodge()) return;
        canMove = false;
        canRotate = false;
        PerformDodge();
    }
    
    private bool CanPerformDodge()
    {
        if (_playerManager.isPerformingAction) return false;
        if (!CLVM.isGrounded) return false;

        return _playerManager.playerStatsManager.UseStamina(30);
    }
    
    private void PerformDodge()
    {
        _playerManager.playerVariableManager.isInvulnerable.Value = true;
        if (CLVM.moveDirection.magnitude > 0.1f)
        {
            if (CLVM.isStrafing == false)
            {
                _playerManager.playerAnimatorManager.PlayTargetActionAnimation(rollForwardHash, true);
            }
            else
            {
                switch (CLVM.moveComposite)
                {
                    case { x: > 0.1f }:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation(rollRightHash, true);
                        break;
                    case { x: < -0.1f }:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation(rollLeftHash, true);
                        break;
                    case { y: > 0.1f }:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation(rollForwardHash, true);
                        break;
                    case { y: < -0.1f }:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation(rollBackwordHash, true);
                        break;
                    default:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation(rollForwardHash, true);
                        break;
                }
            }
        }
        else
        {
            _playerManager.playerAnimatorManager.PlayTargetActionAnimation(backStepHash, true);
        }
    }
    
    public void AttemptToJump()
    {
        if(!CanPerformJump()) return;
        
        if (_canDoubleJump)
        {
            _canDoubleJump = false;
            JumpToJumpState();
            return;
        }
        if (_canJumpLocomotion)
        {
            LocomotionToJumpState();
            return;
        }
        if (_canJumpCrouch)
        {
            CrouchToJumpState();
            return;
        }
    }

    private bool CanPerformJump()
    {
        if (_playerManager.isPerformingAction) return false;

        var currentState = characterManager.characterVariableManager.CLVM.currentState;
        if (currentState == AnimationState.Jump && !_playerManager.playerVariableManager.perkDoubleJump.Value) return false;
        if (currentState == AnimationState.DoubleJump) return false;

        return _playerManager.playerStatsManager.UseStamina();
    }
    
    protected override void EnterJumpState()
    {
        base.EnterJumpState();
        _canDoubleJump = _playerManager.playerVariableManager.perkDoubleJump.Value;
    }
    
    private void ExitFallState()
    {
        _canDoubleJump = false;
    }
    
    public void AttemptToToggleWalk()
    {
        ToggleWalk();
    }
    
    public void AttemptToActivateSprint()
    {
        if (_playerManager.playerVariableManager.isBlock.Value)
        {
            AttemptToDeactivateSprint();
            return;
        }
        
        if (_playerManager.playerVariableManager.stamina.Value >= 10)
        {
            ActivateSprint();
        }
            
    }
    
    public void AttemptToDeactivateSprint()
    {
        DeactivateSprint();
    }
    
    public void AttemptToToggleCrouch()
    {
        ToggleCrouch();
    }

    #endregion
}