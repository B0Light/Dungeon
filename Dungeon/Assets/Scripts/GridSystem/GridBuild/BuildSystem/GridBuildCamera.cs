using UnityEngine;

public class GridBuildCamera : MonoBehaviour
{
    public float moveSpeed = 10f; // 기본 이동 속도
    public float boostMultiplier = 2f; // Shift로 증가하는 이동 속도 배율
    public float zoomSpeed = 10f; // 줌 속도
    public float rotationSpeed = 100f; // 회전 속도
    public float minZoomDistance = 2f; // 최소 줌 거리
    public float maxZoomDistance = 50f; // 최대 줌 거리

    private Vector3 targetPosition; // 카메라가 바라볼 중심점
    private float distance; // 중심점과 카메라의 거리

    private Vector3 _defaultPosition;
    private Quaternion _defaultQuaternion;
    
    // 탑다운 모드 상태 관리용 변수
    private bool isTopDownMode = false;
    private Quaternion _beforeTopDownRotation; // 탑다운 진입 전 회전값 저장

    private void Start()
    {
        targetPosition = transform.position + transform.forward * 10f;
        distance = Vector3.Distance(transform.position, targetPosition);

        _defaultPosition = transform.position;
        _defaultQuaternion = transform.rotation;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
        HandleFreeMove();
    }

    private void HandleMovement()
    {
        if (Input.GetMouseButton(2)) // 중간 버튼으로 이동
        {
            float moveX = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float moveY = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            Vector3 move;

            if (isTopDownMode)
            {
                // 탑다운 모드일 때는 월드 좌표 기준 X(좌우), Z(상하) 평면 이동
                // 카메라가 90도 숙여져 있으므로 transform.up이 월드의 Z축(앞뒤)과 유사함
                move = transform.right * moveX + transform.up * moveY;
            }
            else
            {
                // 기존 방식: 카메라 로컬 좌표 기준 이동
                move = transform.right * moveX + transform.up * moveY;
            }

            transform.position += move;
            targetPosition += move;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minZoomDistance, maxZoomDistance);

            // 탑다운 모드에서도 줌은 타겟 방향(수직)으로 작동해야 하므로 동일 로직 사용 가능
            Vector3 direction = (transform.position - targetPosition).normalized;
            
            // 안전장치: 벡터가 0이 되는 경우 방지 (탑다운에서 타겟과 겹칠 때)
            if (direction == Vector3.zero) direction = isTopDownMode ? Vector3.up : -transform.forward;

            transform.position = targetPosition + direction * distance;
        }
    }

    private void HandleRotation()
    {
        // 탑다운 모드에서는 회전 불가능 (각도 고정 요청)
        if (isTopDownMode) return;

        if (Input.GetMouseButton(1)) // 오른쪽 버튼으로 회전
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.RotateAround(targetPosition, Vector3.up, rotX);
            transform.RotateAround(targetPosition, transform.right, rotY);
            
            Vector3 direction = (transform.position - targetPosition).normalized;
            distance = Vector3.Distance(transform.position, targetPosition);
            transform.LookAt(targetPosition);
        }
    }

    private void HandleFreeMove()
    {
        
        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= boostMultiplier;
        }

        float moveX = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime; // A/D
        float moveZ = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;   // W/S
        
        Vector3 move;

        if (isTopDownMode)
        {
            // 탑다운 모드:
            // W/S (moveZ) -> 월드 상의 앞/뒤 (화면상 위/아래) -> transform.up 사용
            // A/D (moveX) -> 월드 상의 좌/우 (화면상 좌/우) -> transform.right 사용
            // 카메라 각도(90도) 유지
            
            move = transform.right * moveX + transform.up * moveZ;
            
        }
        else
        {
            // 기존 방식: 3D 자유 시점 이동
            float moveY = 0f;
            if (Input.GetKey(KeyCode.Q)) moveY -= currentSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.E)) moveY += currentSpeed * Time.deltaTime;

            move = transform.forward * moveZ + transform.right * moveX + transform.up * moveY;
        }

        transform.position += move;
        targetPosition += move;
    }
    
    public void ResetCamPosition()
    {
        // 리셋 시 탑다운 모드도 해제하는 것이 일반적임
        isTopDownMode = false;

        transform.position = _defaultPosition;
        transform.rotation = _defaultQuaternion;
        
        targetPosition = transform.position + transform.forward * 10f;
        distance = Vector3.Distance(transform.position, targetPosition);
    }
    
    // 토글 스위치 방식으로 변경
    public void SetProjectCam()
    {
        isTopDownMode = !isTopDownMode; // 상태 반전 (True <-> False)

        if (isTopDownMode)
        {
            _beforeTopDownRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            transform.position = new Vector3(targetPosition.x, targetPosition.y + distance, targetPosition.z);
        }
        else
        {
            transform.rotation = _beforeTopDownRotation;
            targetPosition = transform.position + transform.forward * distance;
        }
    }
}