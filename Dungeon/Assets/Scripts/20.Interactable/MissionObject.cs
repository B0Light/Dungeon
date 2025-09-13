using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum MissionObjectType
{
    Door,           // 문 열기/닫기
    Light,          // 라이트 켜기/끄기
    Elevator,       // 엘리베이터 이동
    Platform,       // 플랫폼 이동
    Bridge,         // 다리 펼치기/접기
    Barrier,        // 장벽 활성화/비활성화
    Person,         // 대화 애니메이션 
    Camera,         // vCam 활성화 
    Object,
}

public enum DoorAnimationType
{
    RotateY,        // Y축 회전 (일반 문)
    RotateX,        // X축 회전 (위아래 열리는 문)
    RotateZ,        // Z축 회전 (시계방향 회전)
    SlideHorizontal,// 좌우 슬라이딩
    SlideVertical   // 상하 슬라이딩
}

public class MissionObject : MonoBehaviour
{
    [Header("미션 오브젝트 설정")]
    [SerializeField] private MissionObjectType objectType = MissionObjectType.Door;
    [SerializeField] private bool isActivated = false;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("문 설정 (Door Type Only)")]
    [SerializeField] private DoorAnimationType doorAnimation = DoorAnimationType.RotateY;
    [SerializeField] private Transform leftDoor;              // 왼쪽 문 또는 단일 문
    [SerializeField] private Transform rightDoor;             // 오른쪽 문 (양쪽 문일 때만)
    [SerializeField] private float rotationAngle = 90.0f;     // 회전 각도
    [SerializeField] private float slideDistance = 2.0f;      // 슬라이딩 거리
    
    [Header("라이트 설정 (Light Type Only)")]
    [SerializeField] private Light[] lights;
    [SerializeField] private Material[] emissiveMaterials;    // 발광 재질들
    [SerializeField] private Color activatedColor = Color.white;
    [SerializeField] private Color deactivatedColor = Color.black;
    
    [Header("이동 오브젝트 설정 (Elevator/Platform/Bridge)")]
    [SerializeField] private Transform movableObject;
    [SerializeField] private Vector3 targetPosition;          // 목표 위치 (로컬)
    [SerializeField] private Vector3 targetRotation;          // 목표 회전

    [Header("캐릭터 애니메이션 설정 (Person Type Only)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string activatedAnimationName = "Talk";     // 활성화 시 재생할 애니메이션 이름
    [SerializeField] private string deactivatedAnimationName = "Idle";   // 비활성화 시 재생할 애니메이션 이름
    [SerializeField] private float crossFadeDuration = 0.2f;             // 크로스페이드 지속시간
    [SerializeField] private bool useAnimationTriggers = false;          // 트리거 사용 여부
    [SerializeField] private string activatedTrigger = "StartTalk";      // 활성화 트리거 이름
    [SerializeField] private string deactivatedTrigger = "StopTalk";     // 비활성화 트리거 이름

    [Header("가상 카메라 설정 ")] 
    [SerializeField] private CinemachineVirtualCameraBase vCam;

    [Header("오브젝트  설정 ")] 
    [SerializeField] private GameObject targetObject;
    
    [Header("이벤트")]
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onDeactivated;
    
    // 초기 상태 저장
    private Vector3 _leftDoorInitialPosition;
    private Vector3 _rightDoorInitialPosition;
    private Quaternion _leftDoorInitialRotation;
    private Quaternion _rightDoorInitialRotation;
    private Vector3 _movableObjectInitialPosition;
    private Quaternion _movableObjectInitialRotation;
    
    private bool _isAnimating = false;
    
    private void Awake()
    {
        SaveInitialStates();
    }
    
    private void SaveInitialStates()
    {
        // 문 초기 상태 저장
        if (leftDoor != null)
        {
            _leftDoorInitialPosition = leftDoor.localPosition;
            _leftDoorInitialRotation = leftDoor.localRotation;
        }
        
        if (rightDoor != null)
        {
            _rightDoorInitialPosition = rightDoor.localPosition;
            _rightDoorInitialRotation = rightDoor.localRotation;
        }
        
        // 이동 오브젝트 초기 상태 저장
        if (movableObject != null)
        {
            _movableObjectInitialPosition = movableObject.localPosition;
            _movableObjectInitialRotation = movableObject.localRotation;
        }
        
        // 가상 카메라 초기 상태 설정
        if (vCam != null)
        {
            vCam.Priority = 0;
        }
    }
    
    // 외부에서 호출할 수 있는 공개 메서드들
    public void ActivateObject()
    {
        if (_isAnimating || isActivated) return;
        
        isActivated = true;
        ExecuteObjectAction(true);
        onActivated?.Invoke();
    }
    
    public void DeactivateObject()
    {
        if (_isAnimating || !isActivated) return;
        
        isActivated = false;
        ExecuteObjectAction(false);
        onDeactivated?.Invoke();
    }

    public void PlayAnimation(int code)
    {
        string animationName = "G_0" + code;
        
        if (!string.IsNullOrEmpty(animationName))
        {
            // 애니메이션 상태가 존재하는지 확인
            if (HasAnimationState(animationName))
            {
                animator.CrossFade(animationName, crossFadeDuration);
                Debug.Log($"[MissionObject] Person 애니메이션 CrossFade 실행: {animationName} (duration: {crossFadeDuration})");
            }
            else
            {
                Debug.LogWarning($"[MissionObject] 애니메이션 상태를 찾을 수 없습니다: {animationName}");
            }
        }
        else
        {
            Debug.LogWarning($"[MissionObject] 애니메이션 이름이 비어있습니다. activate: {animationName}");
        }
    }
    
    public void ToggleObject()
    {
        if (isActivated)
            DeactivateObject();
        else
            ActivateObject();
    }
    
    private void ExecuteObjectAction(bool activate)
    {
        switch (objectType)
        {
            case MissionObjectType.Door:
                StartCoroutine(AnimateDoor(activate));
                break;
            case MissionObjectType.Light:
                ControlLights(activate);
                break;
            case MissionObjectType.Elevator:
            case MissionObjectType.Platform:
            case MissionObjectType.Bridge:
                StartCoroutine(AnimateMovableObject(activate));
                break;
            case MissionObjectType.Barrier:
                ControlBarrier(activate);
                break;
            case MissionObjectType.Person:
                ControlPersonAnimation(activate);
                break;
            case MissionObjectType.Camera:
                ControlCamera(activate);
                break;
            case MissionObjectType.Object:
                ControlObject(activate);
                break;
        }
    }
    
    #region Door Animation
    private IEnumerator AnimateDoor(bool opening)
    {
        _isAnimating = true;
        
        switch (doorAnimation)
        {
            case DoorAnimationType.RotateY:
                yield return StartCoroutine(AnimateDoorRotation(opening, Vector3.up));
                break;
            case DoorAnimationType.RotateX:
                yield return StartCoroutine(AnimateDoorRotation(opening, Vector3.right));
                break;
            case DoorAnimationType.RotateZ:
                yield return StartCoroutine(AnimateDoorRotation(opening, Vector3.forward));
                break;
            case DoorAnimationType.SlideHorizontal:
                yield return StartCoroutine(AnimateDoorSliding(opening, true));
                break;
            case DoorAnimationType.SlideVertical:
                yield return StartCoroutine(AnimateDoorSliding(opening, false));
                break;
        }
        
        _isAnimating = false;
    }
    
    private IEnumerator AnimateDoorRotation(bool opening, Vector3 rotationAxis)
    {
        Quaternion leftStartRotation = Quaternion.identity;
        Quaternion rightStartRotation = Quaternion.identity;
        Quaternion leftTargetRotation = Quaternion.identity;
        Quaternion rightTargetRotation = Quaternion.identity;
        
        // 왼쪽 문 설정
        if (leftDoor != null)
        {
            leftStartRotation = leftDoor.localRotation;
            Vector3 rotationVector = rotationAxis * -rotationAngle;
            leftTargetRotation = opening ? 
                _leftDoorInitialRotation * Quaternion.Euler(rotationVector) : 
                _leftDoorInitialRotation;
        }
        
        // 오른쪽 문 설정
        if (rightDoor != null)
        {
            rightStartRotation = rightDoor.localRotation;
            Vector3 rotationVector = rotationAxis * rotationAngle;
            rightTargetRotation = opening ? 
                _rightDoorInitialRotation * Quaternion.Euler(rotationVector) : 
                _rightDoorInitialRotation;
        }
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsedTime / animationDuration);
            
            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Lerp(leftStartRotation, leftTargetRotation, t);
            
            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Lerp(rightStartRotation, rightTargetRotation, t);
            
            yield return null;
        }
        
        // 최종 상태 설정
        if (leftDoor != null)
            leftDoor.localRotation = leftTargetRotation;
        
        if (rightDoor != null)
            rightDoor.localRotation = rightTargetRotation;
    }
    
    private IEnumerator AnimateDoorSliding(bool opening, bool horizontal)
    {
        Vector3 leftStartPos = Vector3.zero;
        Vector3 rightStartPos = Vector3.zero;
        Vector3 leftTargetPos = Vector3.zero;
        Vector3 rightTargetPos = Vector3.zero;
        
        if (leftDoor != null)
        {
            leftStartPos = leftDoor.localPosition;
            Vector3 direction = horizontal ? Vector3.left : Vector3.up;
            leftTargetPos = opening ? 
                _leftDoorInitialPosition + direction * slideDistance : 
                _leftDoorInitialPosition;
        }
        
        if (rightDoor != null)
        {
            rightStartPos = rightDoor.localPosition;
            Vector3 direction = horizontal ? Vector3.right : Vector3.down;
            rightTargetPos = opening ? 
                _rightDoorInitialPosition + direction * slideDistance : 
                _rightDoorInitialPosition;
        }
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsedTime / animationDuration);
            
            if (leftDoor != null)
                leftDoor.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, t);
            
            if (rightDoor != null)
                rightDoor.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, t);
            
            yield return null;
        }
        
        if (leftDoor != null)
            leftDoor.localPosition = leftTargetPos;
        
        if (rightDoor != null)
            rightDoor.localPosition = rightTargetPos;
    }
    #endregion
    
    #region Light Control
    private void ControlLights(bool activate)
    {
        // 라이트 켜기/끄기
        if (lights != null)
        {
            foreach (var light in lights)
            {
                if (light != null)
                    light.enabled = activate;
            }
        }
        
        // 발광 재질 제어
        if (emissiveMaterials != null)
        {
            Color targetColor = activate ? activatedColor : deactivatedColor;
            foreach (var material in emissiveMaterials)
            {
                if (material != null)
                {
                    material.SetColor("_EmissionColor", targetColor);
                    if (activate)
                        material.EnableKeyword("_EMISSION");
                    else
                        material.DisableKeyword("_EMISSION");
                }
            }
        }
    }
    #endregion
    
    #region Movable Object Animation
    private IEnumerator AnimateMovableObject(bool activate)
    {
        if (movableObject == null) yield break;
        
        _isAnimating = true;
        
        Vector3 startPosition = movableObject.localPosition;
        Quaternion startRotation = movableObject.localRotation;
        
        Vector3 endPosition = activate ? targetPosition : _movableObjectInitialPosition;
        Quaternion endRotation = activate ? 
            Quaternion.Euler(targetRotation) : 
            _movableObjectInitialRotation;
        
        float elapsedTime = 0;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsedTime / animationDuration);
            
            movableObject.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            movableObject.localRotation = Quaternion.Lerp(startRotation, endRotation, t);
            
            yield return null;
        }
        
        movableObject.localPosition = endPosition;
        movableObject.localRotation = endRotation;
        
        _isAnimating = false;
    }
    #endregion
    
    #region Barrier Control
    private void ControlBarrier(bool activate)
    {
        // 간단한 활성화/비활성화
        if (movableObject != null)
        {
            movableObject.gameObject.SetActive(activate);
        }
    }
    #endregion
    
    #region Person Animation Control
    private void ControlPersonAnimation(bool activate)
    {
        if (animator == null)
        {
            Debug.LogWarning($"[MissionObject] Person 타입이지만 Animator가 할당되지 않았습니다: {gameObject.name}");
            return;
        }
        
        if (useAnimationTriggers)
        {
            // 트리거 방식 사용
            string triggerName = activate ? activatedTrigger : deactivatedTrigger;
            
            if (!string.IsNullOrEmpty(triggerName))
            {
                animator.SetTrigger(triggerName);
                Debug.Log($"[MissionObject] Person 애니메이션 트리거 실행: {triggerName}");
            }
            else
            {
                Debug.LogWarning($"[MissionObject] 트리거 이름이 비어있습니다. activate: {activate}");
            }
        }
        else
        {
            // CrossFade 방식 사용
            string animationName = activate ? activatedAnimationName : deactivatedAnimationName;
            
            if (!string.IsNullOrEmpty(animationName))
            {
                // 애니메이션 상태가 존재하는지 확인
                if (HasAnimationState(animationName))
                {
                    animator.CrossFade(animationName, crossFadeDuration);
                    Debug.Log($"[MissionObject] Person 애니메이션 CrossFade 실행: {animationName} (duration: {crossFadeDuration})");
                }
                else
                {
                    Debug.LogWarning($"[MissionObject] 애니메이션 상태를 찾을 수 없습니다: {animationName}");
                }
            }
            else
            {
                Debug.LogWarning($"[MissionObject] 애니메이션 이름이 비어있습니다. activate: {activate}");
            }
        }
    }
    
    /// <summary>
    /// Animator에 특정 애니메이션 상태가 존재하는지 확인
    /// </summary>
    /// <param name="stateName">확인할 상태 이름</param>
    /// <returns>상태 존재 여부</returns>
    private bool HasAnimationState(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
        
        // 모든 레이어에서 상태 이름 검색
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            if (animator.HasState(layer, Animator.StringToHash(stateName)))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 현재 재생 중인 애니메이션 이름 가져오기 (디버깅용)
    /// </summary>
    /// <param name="layer">레이어 인덱스 (기본값: 0)</param>
    /// <returns>현재 애니메이션 이름</returns>
    public string GetCurrentAnimationName(int layer = 0)
    {
        if (animator == null) return "";
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.IsName("") ? "Unknown" : "Current Animation Playing";
    }
    
    /// <summary>
    /// Person 타입 전용: 특정 애니메이션을 즉시 재생
    /// </summary>
    /// <param name="animationName">재생할 애니메이션 이름</param>
    /// <param name="fadeDuration">페이드 지속시간 (기본값: crossfadeDuration 사용)</param>
    public void PlayPersonAnimation(string animationName, float? fadeDuration = null)
    {
        if (objectType != MissionObjectType.Person)
        {
            Debug.LogWarning("[MissionObject] PlayPersonAnimation은 Person 타입에서만 사용할 수 있습니다.");
            return;
        }
        
        if (animator == null)
        {
            Debug.LogWarning("[MissionObject] Animator가 할당되지 않았습니다.");
            return;
        }
        
        float duration = fadeDuration ?? crossFadeDuration;
        
        if (HasAnimationState(animationName))
        {
            animator.CrossFade(animationName, duration);
            Debug.Log($"[MissionObject] 수동 애니메이션 재생: {animationName}");
        }
        else
        {
            Debug.LogWarning($"[MissionObject] 애니메이션 상태를 찾을 수 없습니다: {animationName}");
        }
    }
    #endregion

    #region virtual Camera

    private void ControlCamera(bool activate)
    {
        if (vCam != null)
        {
            if (activate)
            {
                StartCoroutine(CameraActivationCoroutine());
            }
            else
            {
                vCam.Priority = 0; // 또는 비활성화하는 다른 방법
            }
        }
    }

    private IEnumerator CameraActivationCoroutine()
    {
        // 카메라 활성화
        vCam.Priority = 30; // Cinemachine Virtual Camera의 경우
        // 또는 vCam.gameObject.SetActive(true); // 일반적인 GameObject 활성화
    
        // 3초 대기
        yield return new WaitForSeconds(3f);
    
        // 카메라 비활성화
        vCam.Priority = 0; // Cinemachine Virtual Camera의 경우
        // 또는 vCam.gameObject.SetActive(false); // 일반적인 GameObject 비활성화
    }

    #endregion

    private void ControlObject(bool activate)
    {
        if(targetObject != null)
            targetObject.SetActive(activate);
    }
    
    // 상태 확인 메서드들
    public bool IsActivated() => isActivated;
    public bool IsAnimating() => _isAnimating;
    public MissionObjectType GetObjectType() => objectType;
}