using System.Collections;
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

    protected override void CalculateMoveDirection()
    {
        PlayerInputManager.Instance.CalculateInput();
        base.CalculateMoveDirection();
    }

    protected override void FaceMoveDirection()
    {
        if(!canRotate) return;
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(CLVM.moveDirection.x, 0f, CLVM.moveDirection.z).normalized;

        if (CLVM.isLockedOn && CLVM.currentLockOnTarget)
        {
            _cameraForward = (CLVM.currentLockOnTarget.transform.position - this.transform.position).normalized;
            _cameraForward.y = 0;
        }
        else
        {
            _cameraForward = PlayerCameraController.Instance.GetCameraForwardZeroedYNormalized();
        }
        
        Quaternion strafingTargetRotation = Quaternion.LookRotation(_cameraForward);

        CLVM.strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;

        CLVM.isTurningInPlace = false;

        if (CLVM.isStrafing)
        {
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
        else
        {
            UpdateStrafeDirection(1f, 0f);
            _cameraRotationOffset = Mathf.Lerp(_cameraRotationOffset, 0f, CLVM.rotationSmoothing * Time.unscaledDeltaTime);

            CLVM.shuffleDirectionZ = 1;
            CLVM.shuffleDirectionX = 0;

            Vector3 faceDirection = new Vector3(CLVM.velocity.x, 0f, CLVM.velocity.z);

            if (faceDirection == Vector3.zero)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(faceDirection),
                CLVM.rotationSmoothing * Time.unscaledDeltaTime
            );
        }
    }

    protected override void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
    {
        base.CalculateRotationalAdditives(leansActivated, headLookActivated, bodyLookActivated);
        float cameraTilt = PlayerCameraController.Instance.GetCameraTiltX();
        cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
        cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);
        CLVM.headLookY = cameraTilt;
        CLVM.bodyLookY = cameraTilt;
    }

    protected override void UpdateBestTarget()
    {
        GameObject newBestTarget = null;
    
        if (CLVM.currentTargetCandidates.Count == 0)
        {
            newBestTarget = null;
        }
        else if (CLVM.currentTargetCandidates.Count == 1)
        {
            newBestTarget = CLVM.currentTargetCandidates[0];
        }
        else
        {
            float bestTargetScore = 0f;

            foreach (GameObject target in CLVM.currentTargetCandidates)
            {
                target.GetComponent<LockOnObject>().Highlight(false);

                Vector3 targetPos = target.transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);
                float distanceScore = 1 / distance * 100;

                Vector3 targetDirection = targetPos - PlayerCameraController.Instance.GetCameraPosition();
                float angleInView = Vector3.Dot(targetDirection.normalized, PlayerCameraController.Instance.GetCameraForward());
                float angleScore = angleInView * 40;

                float totalScore = distanceScore + angleScore;

                if (totalScore > bestTargetScore)
                {
                    bestTargetScore = totalScore;
                    newBestTarget = target;
                }
            }
        }

        if (!CLVM.isLockedOn)
        {
            CLVM.currentLockOnTarget = newBestTarget;
        }
        else
        {
            if (CLVM.currentTargetCandidates.Contains(CLVM.currentLockOnTarget))
            {
                CLVM.currentLockOnTarget.GetComponent<LockOnObject>().Highlight(true);
            }
            else
            {
                CLVM.currentLockOnTarget = newBestTarget;
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
        float moveCoefficient = canMove ? _playerManager.playerVariableManager.GetMoveCoefficient() : 0;
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
    
    #region Lock-on
    public void AddTargetCandidate(GameObject newTarget)
    {
        if (newTarget != null)
        {
            CLVM.currentTargetCandidates.Add(newTarget);
        }
    }
    public void RemoveTarget(GameObject targetToRemove)
    {
        if (CLVM.currentTargetCandidates.Contains(targetToRemove))
        {
            CLVM.currentTargetCandidates.Remove(targetToRemove);
        }
        
        UpdateBestTarget();
        EnableLockOn(CLVM.currentTargetCandidates.Count > 0 && CLVM.isLockedOn);
    }
    
    private void ToggleLockOn()
    {
        UpdateBestTarget();
        //  lock on 대상이 있으며 / 현재 록온 x -> true
        EnableLockOn(CLVM.currentTargetCandidates.Count > 0 && !CLVM.isLockedOn);
    }

    private void EnableLockOn(bool enable)
    {
        CLVM.isLockedOn = enable;
        CLVM.isStrafing = enable ? !CLVM.isSprinting : CLVM.alwaysStrafe ;
        
        PlayerCameraController.Instance.LockOn(enable, CLVM.targetLockOnPos);
        
        if (CLVM.currentLockOnTarget != null)
            CLVM.currentLockOnTarget.GetComponent<LockOnObject>().Highlight(enable);
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
        //if (_playerManager.playerVariableManager.isHungry.Value) return false;
        if (_playerManager.isPerformingAction) return false;
        if (!CLVM.isGrounded) return false;

        return _playerManager.playerStatsManager.UseActionPoint();
    }
    
    private void PerformDodge()
    {
        _playerManager.playerVariableManager.isInvulnerable.Value = true;
        if (CLVM.moveDirection.magnitude > 0.1f)
        {
            if (CLVM.isStrafing == false)
            {
                _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Roll_Forward", true);
            }
            else
            {
                switch (PlayerInputManager.Instance.moveComposite)
                {
                    case var direction when direction.x > 0.1f:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Roll_Right", true);
                        break;
        
                    case var direction when direction.x < -0.1f:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Roll_Left", true);
                        break;
        
                    case var direction when direction.y > 0.1f:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Roll_Forward", true);
                        break;
        
                    case var direction when direction.y < -0.1f:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Roll_Backward", true);
                        break;
        
                    default:
                        _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Roll_Forward", true);
                        break;
                }
            }
        }
        else
        {
            _playerManager.playerAnimatorManager.PlayTargetActionAnimation("Back_Step", true);
        }
    }
    
    public void AttemptToJump()
    {
        if(!CanPerformJump()) return;
        
        _playerManager.playerCombatManager.EnableCanDoJumpingAttack();
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
        if (_playerManager.playerVariableManager.isHungry.Value) return false;
        if (_playerManager.isPerformingAction) return false;

        var currentState = characterManager.characterVariableManager.CLVM.currentState;
        if (currentState == AnimationState.Jump && !_playerManager.playerVariableManager.perkDoubleJump.Value) return false;
        if (currentState == AnimationState.DoubleJump) return false;

        return _playerManager.playerStatsManager.UseActionPoint();
    }
    
    protected override void EnterJumpState()
    {
        base.EnterJumpState();
        _canDoubleJump = _playerManager.playerVariableManager.perkDoubleJump.Value;
    }
    
    private void ExitFallState()
    {
        _playerManager.playerCombatManager.canPerformJumpingAttack = false;
        _canDoubleJump = false;
    }

    public void AttemptToLockOn()
    {
        ToggleLockOn();
    }
    
    public void AttemptToToggleWalk()
    {
        ToggleWalk();
    }
    
    public void AttemptToActivateSprint()
    {
        if (_playerManager.playerVariableManager.isHungry.Value || _playerManager.playerVariableManager.isBlock.Value)
        {
            AttemptToDeactivateSprint();
            return;
        }
        if (_playerManager.playerStatsManager.UseActionPoint())
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