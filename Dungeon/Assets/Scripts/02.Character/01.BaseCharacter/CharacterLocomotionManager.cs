using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterLocomotionManager : MonoBehaviour
{
    protected CharacterManager characterManager;
    
    protected float _cameraRotationOffset = 0f;

    #region Animation Variable Hashes

    private readonly int _movementInputTappedHash = Animator.StringToHash("MovementInputTapped");
    private readonly int _movementInputPressedHash = Animator.StringToHash("MovementInputPressed");
    private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
    private readonly int _shuffleDirectionXHash = Animator.StringToHash("ShuffleDirectionX");
    private readonly int _shuffleDirectionZHash = Animator.StringToHash("ShuffleDirectionZ");

    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");

    private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
    private readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");

    private readonly int _inclineAngleHash = Animator.StringToHash("InclineAngle");

    private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
    private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");

    private readonly int _forwardStrafeHash = Animator.StringToHash("ForwardStrafe");
    protected readonly int _cameraRotationOffsetHash = Animator.StringToHash("CameraRotationOffset");
    private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
    private readonly int _isTurningInPlaceHash = Animator.StringToHash("IsTurningInPlace");

    private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");

    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
    private readonly int _isStartingHash = Animator.StringToHash("IsStarting");

    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

    private readonly int _leanValueHash = Animator.StringToHash("LeanValue");
    private readonly int _headLookXHash = Animator.StringToHash("HeadLookX");
    private readonly int _headLookYHash = Animator.StringToHash("HeadLookY");

    private readonly int _bodyLookXHash = Animator.StringToHash("BodyLookX");
    private readonly int _bodyLookYHash = Animator.StringToHash("BodyLookY");
    
    private readonly int _locomotionStartDirectionHash = Animator.StringToHash("LocomotionStartDirection");

    #endregion
    
    protected const float _ANIMATION_DAMP_TIME = 5f;
    protected const float _STRAFE_DIRECTION_DAMP_TIME = 20f;
    [SerializeField] protected float _targetMaxSpeed;
    protected float _fallStartTime;
    protected float _rotationRate;
    protected float _initialLeanValue;
    protected float _initialTurnValue;
    protected Vector3 _cameraForward;
    protected Vector3 _targetVelocity;
    
    private bool _isSprintingBooster = false;
    private float _boostedSpeed;
    private readonly float _sprintBoostMultiplier = 3f;

    public bool canMove = true;
    public bool canRotate = true;
    
    protected CharacterLocomotionVariableManager CLVM => characterManager.characterVariableManager.CLVM;

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }
    
    protected virtual void Start()
    {
        CLVM.targetLockOnPos = transform.Find("TargetLockOnPos");
        CLVM.isStrafing = CLVM.alwaysStrafe;
        
        canMove = true;
        SwitchState(AnimationState.Locomotion);
    }
    
    #region Walking State

    protected void ToggleWalk()
    {
        EnableWalk(!CLVM.isWalking);
    }

    private void EnableWalk(bool enable)
    {
        CLVM.isWalking = enable && CLVM.isGrounded && !CLVM.isSprinting;
    }

    #endregion

    #region Sprinting State

    protected void ActivateSprint()
    {
        if (!CLVM.isCrouching && !CLVM.isLockedOn)
        {
            EnableWalk(false);
            CLVM.isSprinting = true;
            CLVM.isStrafing = false;
        }
    }

    protected void DeactivateSprint()
    {
        CLVM.isSprinting = false;

        if (CLVM.alwaysStrafe || CLVM.isLockedOn)
        {
            CLVM.isStrafing = true;
        }
    }

    #endregion

    #region Crouching State

    protected void ToggleCrouch()
    {
        if (CLVM.isCrouching)
        {
            DeactivateCrouch();
        }
        else
        {
            ActivateCrouch();
        }
    }

    private void ActivateCrouch()
    {
        CLVM.crouchKeyPressed = true;

        if (CLVM.isGrounded)
        {
            CapsuleCrouchingSize(true);
            DeactivateSprint();
            CLVM.isCrouching = true;
        }
    }

    public void DeactivateCrouch()
    {
        CLVM.crouchKeyPressed = false;

        if (!CLVM.cannotStandUp && !CLVM.isSliding)
        {
            CapsuleCrouchingSize(false);
            CLVM.isCrouching = false;
        }
    }

    /* Animation Event */
    public void ActivateSliding()
    {
        CLVM.isSliding = true;
        CLVM.jumpForce = CLVM.slidingJumpForce;
    }

    public void DeactivateSliding()
    {
        CLVM.isSliding = false;
        CLVM.jumpForce = CLVM.originJumpForce;
    }

    protected virtual void CapsuleCrouchingSize(bool crouching) { }

    #endregion
    
    #region State Change

    private void SwitchState(AnimationState newState)
    {
        ExitCurrentState();
        EnterState(newState);
    }

    protected virtual void EnterState(AnimationState stateToEnter)
    {
        if(characterManager.isDead.Value)
        {
            SwitchState(AnimationState.Dead);
            return;
        }
    }

    protected virtual void ExitCurrentState() { }
    
    protected virtual void EnterLocomotionState() { }
    
    protected virtual void ExitLocomotionState() { }
    
    protected virtual void EnterCrouchState() { }
    
    protected virtual void ExitCrouchState() { }

    protected void CrouchToJumpState()
    {
        if (!CLVM.cannotStandUp)
        {
            DeactivateCrouch();
            SwitchState(AnimationState.Jump);
        }
    }

    #endregion
    
    #region Updates_State
    private void Update()
    {
        switch (CLVM.currentState)
        {
            case AnimationState.Locomotion:
                UpdateLocomotionState();
                break;
            case AnimationState.Jump:
            case AnimationState.DoubleJump:
                UpdateJumpState();
                break;
            case AnimationState.Fall:
                UpdateFallState();
                break;
            case AnimationState.Crouch:
                UpdateCrouchState();
                break;
        }
    }

    protected virtual void UpdateAnimatorController()
    {
        characterManager.animator.SetFloat(_leanValueHash, CLVM.leanValue);
        characterManager.animator.SetFloat(_headLookXHash, CLVM.headLookX);
        characterManager.animator.SetFloat(_headLookYHash, CLVM.headLookY);
        characterManager.animator.SetFloat(_bodyLookXHash, CLVM.bodyLookX);
        characterManager.animator.SetFloat(_bodyLookYHash, CLVM.bodyLookY);

        characterManager.animator.SetFloat(_isStrafingHash, CLVM.isStrafing ? 1.0f : 0.0f);

        characterManager.animator.SetFloat(_inclineAngleHash, CLVM.inclineAngle);

        characterManager.animator.SetFloat(_moveSpeedHash, CLVM.speed2D);
        characterManager.animator.SetInteger(_currentGaitHash, (int) CLVM.currentGait);

        characterManager.animator.SetFloat(_strafeDirectionXHash, CLVM.strafeDirectionX);
        characterManager.animator.SetFloat(_strafeDirectionZHash, CLVM.strafeDirectionZ);
        characterManager.animator.SetFloat(_forwardStrafeHash, CLVM.forwardStrafe);

        characterManager.animator.SetBool(_movementInputHeldHash, CLVM.movementInputHeld);
        characterManager.animator.SetBool(_movementInputPressedHash, CLVM.movementInputPressed);
        characterManager.animator.SetBool(_movementInputTappedHash, CLVM.movementInputTapped);
        characterManager.animator.SetFloat(_shuffleDirectionXHash, CLVM.shuffleDirectionX);
        characterManager.animator.SetFloat(_shuffleDirectionZHash, CLVM.shuffleDirectionZ);

        characterManager.animator.SetBool(_isTurningInPlaceHash, CLVM.isTurningInPlace);
        characterManager.animator.SetBool(_isCrouchingHash, CLVM.isCrouching);

        characterManager.animator.SetFloat(_fallingDurationHash, CLVM.fallingDuration);
        characterManager.animator.SetBool(_isGroundedHash, CLVM.isGrounded);

        characterManager.animator.SetBool(_isWalkingHash, CLVM.isWalking);
        characterManager.animator.SetBool(_isStoppedHash, CLVM.isStopped);

        characterManager.animator.SetFloat(_locomotionStartDirectionHash, CLVM.locomotionStartDirection);
    }

    #endregion
    
    #region Setup

    protected void EnterBaseState()
    {
        CLVM.previousRotation = transform.forward;
    }

    #endregion
    
    #region Movement

    protected virtual void Move() { }

    private void ApplyGravity()
    {
        if (CLVM.velocity.y > Physics.gravity.y)
        {
            CLVM.velocity.y += Physics.gravity.y * CLVM.gravityMultiplier * Time.deltaTime;
        }
    }

    protected virtual void CalculateMoveDirection()
    {
        // 지면에 있지 않음 
        if (!CLVM.isGrounded)
        {
            _targetMaxSpeed = CLVM.currentMaxSpeed;
        }
        // 지면에는 있으나 ...
        else if (CLVM.isCrouching)
        {
            _targetMaxSpeed = CLVM.walkSpeed;
        }
        else if (CLVM.isSprinting)
        {
            if (!_isSprintingBooster)
            {
                // 스프린트를 시작할 때만 실행됨
                _isSprintingBooster = true;
                CLVM.currentMaxSpeed = CLVM.sprintSpeed * _sprintBoostMultiplier;
            }
            _targetMaxSpeed = CLVM.sprintSpeed;
        }
        else
        {
            _targetMaxSpeed = CLVM.isWalking ? CLVM.walkSpeed : CLVM.runSpeed;
        }

        if (!CLVM.isSprinting) _isSprintingBooster = false; // 스프린트 종료 시 상태 초기화

        CLVM.currentMaxSpeed = Mathf.Lerp(CLVM.currentMaxSpeed, _targetMaxSpeed, _ANIMATION_DAMP_TIME * Time.deltaTime);

        _targetVelocity.x = CLVM.moveDirection.x * CLVM.currentMaxSpeed;
        _targetVelocity.z = CLVM.moveDirection.z * CLVM.currentMaxSpeed;

        CLVM.velocity.z = Mathf.Lerp(CLVM.velocity.z, _targetVelocity.z, CLVM.speedChangeDamping * Time.deltaTime);
        CLVM.velocity.x = Mathf.Lerp(CLVM.velocity.x, _targetVelocity.x, CLVM.speedChangeDamping * Time.deltaTime);

        CLVM.speed2D = new Vector3(CLVM.velocity.x, 0f, CLVM.velocity.z).magnitude;
        CLVM.speed2D = Mathf.Round(CLVM.speed2D * 1000f) / 1000f;
        
        Vector3 playerForwardVector = transform.forward;

        CLVM.newDirectionDifferenceAngle = playerForwardVector != CLVM.moveDirection
            ? Vector3.SignedAngle(playerForwardVector, CLVM.moveDirection, Vector3.up)
            : 0f;

        CalculateGait();
    }


    private void CalculateGait()
    {
        float runThreshold = (CLVM.walkSpeed + CLVM.runSpeed) / 2;
        float sprintThreshold = (CLVM.runSpeed + CLVM.sprintSpeed) / 2;

        if (CLVM.speed2D < 0.01)
        {
            CLVM.currentGait = GaitState.Idle;
        }
        else if (CLVM.speed2D < runThreshold)
        {
            CLVM.currentGait = GaitState.Walk;
        }
        else if (CLVM.speed2D < sprintThreshold)
        {
            CLVM.currentGait = GaitState.Run;
        }
        else
        {
            CLVM.currentGait = GaitState.Sprint;
        }
    }

    protected virtual void FaceMoveDirection() { }

    private void CheckIfStopped()
    {
        CLVM.isStopped = CLVM.moveDirection.magnitude == 0 && CLVM.speed2D < 0.5;
    }

    private void CheckIfStarting()
    {
        CLVM.locomotionStartTimer = VariableOverrideDelayTimer(CLVM.locomotionStartTimer);

        bool isStartingCheck = false;

        if (CLVM.locomotionStartTimer <= 0.0f)
        {
            if (CLVM.moveDirection.magnitude > 0.01 && CLVM.speed2D < 1 && !CLVM.isStrafing)
            {
                isStartingCheck = true;
            }

            if (isStartingCheck)
            {
                if (!CLVM.isStarting)
                {
                    CLVM.locomotionStartDirection = CLVM.newDirectionDifferenceAngle;
                    characterManager.animator.SetFloat(_locomotionStartDirectionHash, CLVM.locomotionStartDirection);
                }

                float delayTime = 0.2f;
                CLVM.leanDelay = delayTime;
                CLVM.headLookDelay = delayTime;
                CLVM.bodyLookDelay = delayTime;

                CLVM.locomotionStartTimer = delayTime;
            }
        }
        else
        {
            isStartingCheck = true;
        }

        CLVM.isStarting = isStartingCheck;
        characterManager.animator.SetBool(_isStartingHash, CLVM.isStarting);
    }

    protected void UpdateStrafeDirection(float targetZ, float targetX)
    {
        CLVM.strafeDirectionZ = Mathf.Lerp(CLVM.strafeDirectionZ, targetZ, _ANIMATION_DAMP_TIME * Time.deltaTime);
        CLVM.strafeDirectionX = Mathf.Lerp(CLVM.strafeDirectionX, targetX, _ANIMATION_DAMP_TIME * Time.deltaTime);
        CLVM.strafeDirectionZ = Mathf.Round(CLVM.strafeDirectionZ * 1000f) / 1000f;
        CLVM.strafeDirectionX = Mathf.Round(CLVM.strafeDirectionX * 1000f) / 1000f;
    }

    #endregion
    
    #region Ground Checks

    protected virtual void GroundedCheck()
    {
        if (CLVM.isGrounded)
        {
            GroundInclineCheck();
        }
    }

    private void GroundInclineCheck()
    {
        float rayDistance = Mathf.Infinity;
        CLVM.rearRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
        CLVM.frontRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);

        Physics.Raycast(CLVM.rearRayPos.position, CLVM.rearRayPos.TransformDirection(-Vector3.up), out RaycastHit rearHit, rayDistance, CLVM.groundLayerMask);
        Physics.Raycast(
            CLVM.frontRayPos.position,
            CLVM.frontRayPos.TransformDirection(-Vector3.up),
            out RaycastHit frontHit,
            rayDistance,
            CLVM.groundLayerMask
        );

        Vector3 hitDifference = frontHit.point - rearHit.point;
        float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

        CLVM.inclineAngle = Mathf.Lerp(CLVM.inclineAngle, Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
    }

    private void CeilingHeightCheck()
    {
        float rayDistance = Mathf.Infinity;
        float minimumStandingHeight = CLVM.capsuleStandingHeight - CLVM.frontRayPos.localPosition.y;

        Vector3 midpoint = new Vector3(transform.position.x, transform.position.y + CLVM.frontRayPos.localPosition.y, transform.position.z);
        if (Physics.Raycast(midpoint, transform.TransformDirection(Vector3.up), out RaycastHit ceilingHit, rayDistance, CLVM.groundLayerMask))
        {
            CLVM.cannotStandUp = ceilingHit.distance < minimumStandingHeight;
        }
        else
        {
            CLVM.cannotStandUp = false;
        }
    }

    #endregion
    
    #region Falling

    private void ResetFallingDuration()
    {
        _fallStartTime = Time.time;
        CLVM.fallingDuration = 0f;
    }

    private void UpdateFallingDuration()
    {
        CLVM.fallingDuration = Time.time - _fallStartTime;
    }

    #endregion

    #region Checks

    private void CheckEnableTurns()
    {
        CLVM.headLookDelay = VariableOverrideDelayTimer(CLVM.headLookDelay);
        CLVM.enableHeadTurn = CLVM.headLookDelay == 0.0f && !CLVM.isStarting;
        CLVM.bodyLookDelay = VariableOverrideDelayTimer(CLVM.bodyLookDelay);
        CLVM.enableBodyTurn = CLVM.bodyLookDelay == 0.0f && !(CLVM.isStarting || CLVM.isTurningInPlace);
    }

    private void CheckEnableLean()
    {
        CLVM.leanDelay = VariableOverrideDelayTimer(CLVM.leanDelay);
        CLVM.enableLean = CLVM.leanDelay == 0.0f && !(CLVM.isStarting || CLVM.isTurningInPlace);
    }

    #endregion

    #region Lean and Offsets

    protected virtual void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
    {
        if (headLookActivated || leansActivated || bodyLookActivated)
        {
            CLVM.currentRotation = transform.forward;

            _rotationRate = CLVM.currentRotation != CLVM.previousRotation
                ? Vector3.SignedAngle(CLVM.currentRotation, CLVM.previousRotation, Vector3.up) / Time.deltaTime * -1f
                : 0f;
        }

        _initialLeanValue = leansActivated ? _rotationRate : 0f;

        float leanSmoothness = 5;
        float maxLeanRotationRate = 275.0f;

        float referenceValue = CLVM.speed2D / CLVM.sprintSpeed;
        CLVM.leanValue = CalculateSmoothedValue(
            CLVM.leanValue,
            _initialLeanValue,
            maxLeanRotationRate,
            leanSmoothness,
            CLVM.leanCurve,
            referenceValue,
            true
        );

        float headTurnSmoothness = 5f;

        if (headLookActivated && CLVM.isTurningInPlace)
        {
            _initialTurnValue = 0;
            CLVM.headLookX = Mathf.Lerp(CLVM.headLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
        }
        else
        {
            _initialTurnValue = headLookActivated ? _rotationRate : 0f;
            CLVM.headLookX = CalculateSmoothedValue(
                CLVM.headLookX,
                _initialTurnValue,
                maxLeanRotationRate,
                headTurnSmoothness,
                CLVM.headLookXCurve,
                CLVM.headLookX,
                false
            );
        }

        float bodyTurnSmoothness = 5f;

        _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;

        CLVM.bodyLookX = CalculateSmoothedValue(
            CLVM.bodyLookX,
            _initialTurnValue,
            maxLeanRotationRate,
            bodyTurnSmoothness,
            CLVM.bodyLookXCurve,
            CLVM.bodyLookX,
            false
        );
        CLVM.previousRotation = CLVM.currentRotation;
    }

    protected float CalculateSmoothedValue(
        float mainVariable,
        float newValue,
        float maxRateChange,
        float smoothness,
        AnimationCurve referenceCurve,
        float referenceValue,
        bool isMultiplier
    )
    {
        float changeVariable = newValue / maxRateChange;

        changeVariable = Mathf.Clamp(changeVariable, -1.0f, 1.0f);

        if (isMultiplier)
        {
            float multiplier = referenceCurve.Evaluate(referenceValue);
            changeVariable *= multiplier;
        }
        else
        {
            changeVariable = referenceCurve.Evaluate(changeVariable);
        }

        if (!changeVariable.Equals(mainVariable))
        {
            changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);
        }

        return changeVariable;
    }

    private float VariableOverrideDelayTimer(float timeVariable)
    {
        if (timeVariable > 0.0f)
        {
            timeVariable -= Time.deltaTime;
            timeVariable = Mathf.Clamp(timeVariable, 0.0f, 1.0f);
        }
        else
        {
            timeVariable = 0.0f;
        }

        return timeVariable;
    }

    #endregion

    #region Lock-on System
    protected virtual void UpdateBestTarget() { }

    #endregion
    
    #region Locomotion State

    public void SetLocomotionState()
    {
        SwitchState(AnimationState.Locomotion);
    }

    private void UpdateLocomotionState()
    {
        GroundedCheck();

        if (!CLVM.isGrounded)
        {
            SwitchState(AnimationState.Fall);
        }

        if (CLVM.isCrouching)
        {
            SwitchState(AnimationState.Crouch);
        }

        CheckEnableTurns();
        CheckEnableLean();
        CalculateRotationalAdditives(CLVM.enableLean, CLVM.enableHeadTurn, CLVM.enableBodyTurn);

        CalculateMoveDirection();
        CheckIfStarting();
        CheckIfStopped();
        FaceMoveDirection();
        Move();
        UpdateAnimatorController();
    }

    protected void LocomotionToJumpState()
    {
        SwitchState(AnimationState.Jump);
    }

    #endregion

    #region Jump State

    protected void JumpToJumpState()
    {
        if(characterManager.isPerformingAction) return;
        SwitchState(AnimationState.DoubleJump);
        characterManager.characterAnimatorManager.PlayTargetActionAnimation("JumpTwice", true, canMove:true, canRotate:true);
    }
    
    protected void EnterDoubleJumpState()
    {
        CLVM.isSliding = false;

        CLVM.velocity = new Vector3(CLVM.velocity.x, CLVM.jumpForce, CLVM.velocity.z);
        CLVM.jumpForce = CLVM.originJumpForce;
    }

    protected virtual void EnterJumpState()
    {
        characterManager.animator.SetBool(_isJumpingAnimHash, true);

        CLVM.isSliding = false;

        CLVM.velocity = new Vector3(CLVM.velocity.x, CLVM.jumpForce, CLVM.velocity.z);
        CLVM.jumpForce = CLVM.originJumpForce;
    }

    private void UpdateJumpState()
    {
        ApplyGravity();

        if (CLVM.velocity.y <= 0f)
        {
            characterManager.animator.SetBool(_isJumpingAnimHash, false);
            SwitchState(AnimationState.Fall);
        }

        GroundedCheck();

        CalculateRotationalAdditives(false, CLVM.enableHeadTurn, CLVM.enableBodyTurn);
        CalculateMoveDirection();
        FaceMoveDirection();
        Move();
        UpdateAnimatorController();
    }

    protected virtual void ExitJumpState()
    {
        characterManager.animator.SetBool(_isJumpingAnimHash, false);
    }

    protected virtual void EnterDeadState()
    {
        canMove = false;
        canRotate = false;
    }

    #endregion

    #region Fall State

    protected void EnterFallState()
    {
        ResetFallingDuration();
        CLVM.velocity.y = 0f;

        DeactivateCrouch();
        CLVM.isSliding = false;
    }

    private void UpdateFallState()
    {
        GroundedCheck();

        CalculateRotationalAdditives(false, CLVM.enableHeadTurn, CLVM.enableBodyTurn);

        CalculateMoveDirection();
        FaceMoveDirection();

        ApplyGravity();
        Move();
        UpdateAnimatorController();

        if (CLVM.isGrounded)
        {
            SwitchState(AnimationState.Locomotion);
        }

        UpdateFallingDuration();
    }

    #endregion

    #region Crouch State

    protected virtual void UpdateCrouchState()
    {
        GroundedCheck();
        if (!CLVM.isGrounded)
        {
            DeactivateCrouch();
            CapsuleCrouchingSize(false);
            SwitchState(AnimationState.Fall);
        }

        CeilingHeightCheck();

        if (!CLVM.crouchKeyPressed && !CLVM.cannotStandUp)
        {
            DeactivateCrouch();
            SwitchToLocomotionState();
        }

        if (!CLVM.isCrouching)
        {
            CapsuleCrouchingSize(false);
            SwitchToLocomotionState();
        }

        CheckEnableTurns();
        CheckEnableLean();

        CalculateRotationalAdditives(false, CLVM.enableHeadTurn, false);

        CalculateMoveDirection();
        CheckIfStarting();
        CheckIfStopped();

        FaceMoveDirection();
        Move();
        UpdateAnimatorController();
    }

    private void SwitchToLocomotionState()
    {
        DeactivateCrouch();
        SwitchState(AnimationState.Locomotion);
    }

    #endregion
    
}

