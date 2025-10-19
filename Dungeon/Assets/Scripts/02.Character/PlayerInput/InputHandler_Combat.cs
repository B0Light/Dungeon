using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_Combat : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private PlayerControls _playerControls;
    
    private Vector2 _startPosition;
    private float _startTime;

    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 1f;
    
    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void Start()
    {
        InputHandlerManager.Instance.RegisterAndEnableHandler(this);
    }

    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public void EnableInput()
    {
        _playerControls.Combat.Enable();
        _playerControls.Combat.Touch.started += OnTouchStart;
        _playerControls.Combat.Touch.canceled += OnTouchEnd;
    }

    public void DisableInput()
    {
        _playerControls.Combat.Disable();
        _playerControls.Combat.Touch.started -= OnTouchStart;
        _playerControls.Combat.Touch.canceled -= OnTouchEnd;
    }
    
    private void OnTouchStart(InputAction.CallbackContext context)
    {
        _startPosition = _playerControls.Combat.Position.ReadValue<Vector2>();
        _startTime = Time.time;
    }
    
    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        Vector2 endPosition = _playerControls.Combat.Position.ReadValue<Vector2>();
        float endTime = Time.time;

        float swipeDistance = Vector2.Distance(_startPosition, endPosition);
        float swipeTime = endTime - _startTime;

        if (swipeDistance > minSwipeDistance && swipeTime < maxSwipeTime)
        {
            Vector2 swipeDirection = (endPosition - _startPosition).normalized;
            DetectSwipeDirection(swipeDirection);
        }
    }

    private void DetectSwipeDirection(Vector2 swipeVector)
    {
        float angle = Vector2.SignedAngle(Vector2.up, swipeVector);
        Dir attackDir = Dir.Up;
        
        if (angle < 0)
        {
            angle = 360 + angle;
        }

        if (angle >= 337.5f || angle < 22.5f)
        {
            attackDir = Dir.Up;
            Debug.Log("Swipe Up");
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            attackDir = Dir.UpRight;
            Debug.Log("Swipe Up-Right");
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            attackDir = Dir.Right;
            Debug.Log("Swipe Right");
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            attackDir = Dir.DownRight;
            Debug.Log("Swipe Down-Right");
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            attackDir = Dir.Down;
            Debug.Log("Swipe Down");
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            attackDir = Dir.DownLeft;
            Debug.Log("Swipe Down-Left");
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            attackDir = Dir.Left;
            Debug.Log("Swipe Left");
        }
        else if (angle >= 292.5f && angle < 337.5f)
        {
            attackDir = Dir.UpLeft;
            Debug.Log("Swipe Up-Left");
        }
        
        _playerManager.playerCombatManager.PerformWeaponDirAction(attackDir);
    }
    
    private void OnDestroy()
    {
        if (InputHandlerManager.Instance != null)
        {
            InputHandlerManager.Instance.UnregisterAndDisableHandler(this);
        }
    }
}
