using System.Collections.Generic;
using UnityEngine;

public class CharacterLocomotionVariableManager : MonoBehaviour
{
    [Header("Main Settings")]
    public bool alwaysStrafe = false;
    public float walkSpeed = 2.5f;
    public float runSpeed = 6f;
    public float sprintSpeed = 12f;
    public float speedChangeDamping = 10f;
    public float rotationSmoothing = 10f;
    [Header("Shuffles")]
    public float shuffleDirectionX;
    public float shuffleDirectionZ;
    [Header("Capsule Values")]
    public float capsuleStandingHeight = 1.8f;
    public float capsuleStandingCentre = 0.93f;
    public float capsuleCrouchingHeight = 1.2f;
    public float capsuleCrouchingCentre = 0.6f;
    [Header("Strafing")]
    public float forwardStrafeMinThreshold = -55.0f;
    public float forwardStrafeMaxThreshold = 125.0f;
    public float forwardStrafe = 1f;
    [Header("Grounded Angle")]
    public Transform rearRayPos;
    public Transform frontRayPos;
    public LayerMask groundLayerMask;
    public float inclineAngle;
    public float groundedOffset = -0.14f;
    [Header("In-Air")]
    public float jumpForce = 8f;
    public float originJumpForce = 8f;
    public float slidingJumpForce = 13f;
    public float gravityMultiplier = 2f;
    public float fallingDuration;
    [Header("Head Look")]
    public bool enableHeadTurn = true;
    public float headLookDelay;
    public float headLookX;
    public float headLookY;
    public AnimationCurve headLookXCurve;
    [Header("Body Look")]
    public bool enableBodyTurn = true;
    public float bodyLookDelay;
    public float bodyLookX;
    public float bodyLookY;
    public AnimationCurve bodyLookXCurve;
    [Header("Player Lean")]
    public bool enableLean = true;
    public float leanDelay; 
    public float leanValue;
    public AnimationCurve leanCurve;
    public float leansHeadLooksDelay;
    public bool animationClipEnd;
    
    [Header("RunTimeProperty")]
    public List<GameObject> currentTargetCandidates = new List<GameObject>();
    public AnimationState currentState = AnimationState.Base;
    [HideInInspector] public bool cannotStandUp;
    [HideInInspector] public bool crouchKeyPressed;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool isGrounded = true;
    [HideInInspector] public bool isLockedOn;
    [HideInInspector] public bool isSliding;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isStarting;
    [HideInInspector] public bool isStopped = true;
    [HideInInspector] public bool isStrafing;
    [HideInInspector] public bool isTurningInPlace;
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool movementInputHeld;
    [HideInInspector] public bool movementInputPressed;
    [HideInInspector] public bool movementInputTapped;
    
    [HideInInspector] public float currentMaxSpeed;
    [HideInInspector] public float locomotionStartDirection;
    [HideInInspector] public float locomotionStartTimer;
    [HideInInspector] public float lookingAngle;
    [HideInInspector] public float newDirectionDifferenceAngle;
    [HideInInspector] public float speed2D;
    [HideInInspector] public float strafeAngle;
    [HideInInspector] public float strafeDirectionX;
    [HideInInspector] public float strafeDirectionZ;
    
    [HideInInspector] public GameObject currentLockOnTarget;
    [HideInInspector] public GaitState currentGait;
    [HideInInspector] public Transform targetLockOnPos;
    [HideInInspector] public Vector3 currentRotation = new Vector3(0f, 0f, 0f);
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public Vector3 previousRotation; 
    [HideInInspector] public Vector3 velocity;
}
