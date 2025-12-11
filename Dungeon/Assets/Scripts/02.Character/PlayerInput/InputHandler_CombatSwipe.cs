using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler_CombatSwipe : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private PlayerControls _playerControls;
    private Vector2 _startPosition;
    private float _startTime;
    
    [SerializeField] private GameObject swipeTrailPrefab;
    private GameObject _currentSwipeTrail;
    
    // 터치 상태를 추적하는 변수 추가
    private bool _isSwiping = false;

    private readonly float _minSwipeDistance = 50f;
    private readonly float _maxSwipeTime = 1f;
    
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public void Register(PlayerControls playerControls)
    {
        Debug.Log("[Register] InputHandler_Combat");
        _playerControls = playerControls;
        playerControls.Combat_Touch.Enable();
        playerControls.Combat_Touch.Touch.started += OnTouchStart;
        playerControls.Combat_Touch.Touch.canceled += OnTouchEnd;
        playerControls.Combat_Touch.Position.performed += OnTouchMove;
        playerControls.Combat_Touch.Dodge.performed += OnDodge;
    }

    public void Unregister(PlayerControls playerControls)
    {
        Debug.Log("[Unregister] InputHandler_Combat");
        _playerControls = null;
        playerControls.Combat_Touch.Disable();
        playerControls.Combat_Touch.Touch.started -= OnTouchStart;
        playerControls.Combat_Touch.Touch.canceled -= OnTouchEnd;
        playerControls.Combat_Touch.Position.performed -= OnTouchMove;
        playerControls.Combat_Touch.Dodge.performed -= OnDodge;
    }
    
    
    private void OnTouchStart(InputAction.CallbackContext context)
    {
        _startPosition = _playerControls.Combat_Touch.Position.ReadValue<Vector2>();
        _startTime = Time.time;
        
        // 스와이프 시작 상태로 설정
        _isSwiping = true;

        _currentSwipeTrail = Instantiate(swipeTrailPrefab);
        UpdateTrailPosition(_startPosition);
    }
    
    private void OnTouchMove(InputAction.CallbackContext context)
    {
        // _isSwiping이 true일 때만 위치 업데이트
        if (_isSwiping && _currentSwipeTrail != null)
        {
            Vector2 screenPos = context.ReadValue<Vector2>();
            UpdateTrailPosition(screenPos);
        }
    }
    
    private void UpdateTrailPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 5f));
        _currentSwipeTrail.transform.position = worldPosition;
    }
    
    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        // 스와이프 종료 상태로 설정
        _isSwiping = false;

        if (_currentSwipeTrail != null)
        {
            // 트레일이 자동으로 사라지도록 설정
            _currentSwipeTrail.GetComponent<TrailRenderer>().autodestruct = true;
            _currentSwipeTrail.transform.parent = null;
        }
        
        Vector2 endPosition = _playerControls.Combat_Touch.Position.ReadValue<Vector2>();
        float endTime = Time.time;

        float swipeDistance = Vector2.Distance(_startPosition, endPosition);
        float swipeTime = endTime - _startTime;

        if (swipeDistance > _minSwipeDistance && swipeTime < _maxSwipeTime)
        {
            Vector2 swipeDirection = (endPosition - _startPosition).normalized;
            DetectSwipeDirection(swipeDirection);
        }
    }

    private void DetectSwipeDirection(Vector2 swipeVector)
    {
        float angle = Vector2.SignedAngle(Vector2.up, swipeVector);
        Dir dir = Dir.Down;
        if (angle < 0)
        {
            angle = 360 + angle;
        }

        if (angle >= 337.5f || angle < 22.5f)
        {
            Debug.Log("Swipe Up");
            dir = Dir.Up;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            Debug.Log("Swipe Up-Left");
            dir = Dir.UpLeft;
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            Debug.Log("Swipe Left");
            dir = Dir.Left;
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            Debug.Log("Swipe Down-Left");
            dir = Dir.DownLeft;
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            Debug.Log("Swipe Down");
            dir = Dir.Down;
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            Debug.Log("Swipe Down-Right");
            dir = Dir.DownRight;
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            Debug.Log("Swipe Right");
            dir = Dir.Right;
        }
        else if (angle >= 292.5f && angle < 337.5f)
        {
            Debug.Log("Swipe Up-Right");
            dir = Dir.UpRight;
        }
        PlayAnimation(dir);
    }

    private void PlayAnimation(Dir dir)
    {
        _playerManager.playerCombatManager.Attack(dir);
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        var dir = context.ReadValue<float>();
        _playerManager.playerCombatManager.Dodge(dir);
    }
}
