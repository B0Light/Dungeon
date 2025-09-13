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
            // 월드 좌표 기준이 아닌, 카메라 좌표 기준으로 이동
            float moveX = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float moveY = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            // 카메라의 현재 방향에 따라 이동
            Vector3 move = transform.right * moveX + transform.up * moveY;
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

            // 카메라에서 타겟으로의 방향 벡터 사용
            Vector3 direction = (transform.position - targetPosition).normalized;
            transform.position = targetPosition + direction * distance;
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // 오른쪽 버튼으로 회전
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float rotY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // 카메라 회전을 현재 방향 기준으로 적용
            transform.RotateAround(targetPosition, Vector3.up, rotX);
            transform.RotateAround(targetPosition, transform.right, rotY);
            
            // 회전 후 거리와 위치 업데이트
            Vector3 direction = (transform.position - targetPosition).normalized;
            distance = Vector3.Distance(transform.position, targetPosition);
            transform.LookAt(targetPosition);
        }
    }

    private void HandleFreeMove()
    {
        if (Input.GetMouseButton(1)) // 오른쪽 버튼 누른 상태에서 이동
        {
            float currentSpeed = moveSpeed;

            // 왼쪽 Shift 키로 속도 증가
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed *= boostMultiplier;
            }

            float moveX = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime; // A/D
            float moveZ = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;   // W/S

            // Q/E 키를 통한 Y축 이동
            float moveY = 0f;
            if (Input.GetKey(KeyCode.Q))
            {
                moveY -= currentSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.E))
            {
                moveY += currentSpeed * Time.deltaTime;
            }

            // 카메라 기준 이동 방향
            Vector3 move = transform.forward * moveZ + transform.right * moveX + transform.up * moveY;
            transform.position += move;
            targetPosition += move;
        }
    }


    public void ResetCamPosition()
    {
        transform.position = _defaultPosition;
        transform.rotation = _defaultQuaternion;
        
        targetPosition = transform.position + transform.forward * 10f;
        distance = Vector3.Distance(transform.position, targetPosition);
    }
}