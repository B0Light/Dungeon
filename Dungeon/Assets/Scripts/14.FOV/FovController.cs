using UnityEngine;
using UnityEngine.InputSystem;

public class FovController : MonoBehaviour {

	[SerializeField] private float moveSpeed = 6;
	[SerializeField] private FieldOfView_Sight fieldOfViewSight;
	[SerializeField] private Vector3 offset = Vector3.up;
    
    private CharacterController _controller; 
    private PlayerControls _controls; 
	private Camera _vCam;
    
	private Vector3 _movementInput = Vector3.zero; 
	private Vector2 _mousePositionInput = Vector2.zero;
    private float _gravity = -9.81f;
    private Vector3 _velocity;

	void Awake () {
        _controls = new PlayerControls();
	}

	void Start () {
		_controller = GetComponent<CharacterController> ();
		_vCam = Camera.main;
	}

    void OnEnable()
    {
        _controls.Enable();
        
        _controls.FOVPlayer.Move.performed += OnMovePerformed;
        _controls.FOVPlayer.Move.canceled += OnMoveCanceled;
        
        // 🌟 Look 액션에 입력 처리 로직을 완전히 통합합니다.
        _controls.FOVPlayer.Look.performed += OnLookPerformed;
        _controls.FOVPlayer.Look.canceled += OnLookPerformed;
    }

    void OnDisable()
    {
        _controls.Disable();
        
        _controls.FOVPlayer.Move.performed -= OnMovePerformed;
        _controls.FOVPlayer.Move.canceled -= OnMoveCanceled;
        
        _controls.FOVPlayer.Look.performed -= OnLookPerformed;
        _controls.FOVPlayer.Look.canceled -= OnLookPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _movementInput = new Vector3(input.x, 0, input.y).normalized;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _movementInput = Vector3.zero;
    }
    
    private void OnLookPerformed(InputAction.CallbackContext context)
    {
	    _mousePositionInput = context.ReadValue<Vector2>();
    }
    
	void Update ()
	{
		UpdateSight();
		UpdateGravity();
		UpdateMove();
	}
	
	private void UpdateSight()
	{
		Ray ray = _vCam.ScreenPointToRay(_mousePositionInput);
		
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero); 
		if (groundPlane.Raycast(ray, out float distance)) {
			
			Vector3 mousePos = ray.GetPoint(distance);
			Vector3 lookTarget = new Vector3(mousePos.x, transform.position.y, mousePos.z);
			transform.LookAt (lookTarget);

			Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
			fieldOfViewSight.SetAimDirection(mousePos - playerPos);
			fieldOfViewSight.SetOrigin(playerPos + offset);
		}
	}

	private void UpdateGravity()
	{
		if (_controller.isGrounded)
			_velocity.y = 0f;
		else
			_velocity.y += _gravity * Time.deltaTime;
	}

	private void UpdateMove()
	{
		Vector3 move = _movementInput * moveSpeed;
		Vector3 finalMovement = move + _velocity;
		_controller.Move(finalMovement * Time.deltaTime);
	}

	
}