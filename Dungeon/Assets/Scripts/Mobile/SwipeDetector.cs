using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetector : MonoBehaviour
{
    private PlayerControls _controls;
    private Vector2 _startPosition;
    private float _startTime;

    [SerializeField] private GameObject swipeTrailPrefab;
    private GameObject _currentSwipeTrail;
    
    // 터치 상태를 추적하는 변수 추가
    private bool _isSwiping = false;

    public float minSwipeDistance = 50f;
    public float maxSwipeTime = 1f;

    void Awake()
    {
        _controls = new PlayerControls();
    }

    void OnEnable()
    {
        _controls.Enable();
        _controls.Combat.Touch.started += OnTouchStart;
        _controls.Combat.Touch.canceled += OnTouchEnd;
        _controls.Combat.Position.performed += OnTouchMove;
    }

    void OnDisable()
    {
        _controls.Disable();
        _controls.Combat.Touch.started -= OnTouchStart;
        _controls.Combat.Touch.canceled -= OnTouchEnd;
        _controls.Combat.Position.performed -= OnTouchMove;
    }
    
    private void OnTouchStart(InputAction.CallbackContext context)
    {
        _startPosition = _controls.Combat.Position.ReadValue<Vector2>();
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
            Vector2 screenPos = _controls.Combat.Position.ReadValue<Vector2>();
            UpdateTrailPosition(screenPos);
        }
    }
    
    private void UpdateTrailPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
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
        
        Vector2 endPosition = _controls.Combat.Position.ReadValue<Vector2>();
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

        if (angle < 0)
        {
            angle = 360 + angle;
        }

        if (angle >= 337.5f || angle < 22.5f)
        {
            Debug.Log("Swipe Up");
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            Debug.Log("Swipe Up-Right");
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            Debug.Log("Swipe Right");
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            Debug.Log("Swipe Down-Right");
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            Debug.Log("Swipe Down");
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            Debug.Log("Swipe Down-Left");
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            Debug.Log("Swipe Left");
        }
        else if (angle >= 292.5f && angle < 337.5f)
        {
            Debug.Log("Swipe Up-Left");
        }
    }
}