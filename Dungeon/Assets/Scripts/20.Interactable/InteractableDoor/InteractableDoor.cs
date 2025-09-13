using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum DoorType
{
    Horizontal,    // 가로 미닫이 문
    Vertical,      // 세로 미닫이 문  
    Rotate,        // 회전하는 문
    Brake,         // 파괴되는 문
}

public class InteractableDoor : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private DoorType doorType = DoorType.Horizontal;
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Movement Settings")]
    [SerializeField] private float slideDistance = 2.0f;      // 미닫이 문 이동 거리
    [SerializeField] private float rotationAngle = 90.0f;     // 회전 각도
    
    [Header("Door Components")]
    [SerializeField] private Transform leftDoor;              // 왼쪽/위쪽 문 (또는 단일 문)
    [SerializeField] private Transform rightDoor;             // 오른쪽/아래쪽 문 (양쪽 문일 때만 사용)

    [SerializeField] private int keyItemCode = 0;
    private readonly int _masterKeyCode = 1299;
    private bool _isOpen;
    private bool _isLock;
    private bool _isAnimating;
    
    // 초기 위치/회전 저장
    private Vector3 _leftDoorInitialPosition;
    private Vector3 _rightDoorInitialPosition;
    private Quaternion _leftDoorInitialRotation;
    private Quaternion _rightDoorInitialRotation;

    [SerializeField] private EffectPlayer effectPlayer;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 초기 위치/회전 저장
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
        
        effectPlayer?.SetResource();
    }
    
    protected virtual void Start()
    {
        _isOpen = false;
        _isAnimating = false;
        
        if (keyItemCode != 0)
        {
            _isLock = true;
            interactableText = "[잠김] " + WorldDatabase_Item.Instance.GetItemByID(keyItemCode).itemName + " 필요";
        }
        else
        {
            _isLock = false;
            interactableText = "문 열기";
        }
    }
    
    public override void Interact(PlayerManager player)
    {
        if (_isAnimating) return; // 애니메이션 중이면 상호작용 차단
        
        base.Interact(player);
        
        if (_isLock)
        {
            if (WorldPlayerInventory.Instance.RemoveItemInInventory(keyItemCode) || 
                WorldPlayerInventory.Instance.RemoveItemInInventory(_masterKeyCode))
            {
                _isLock = false;
                ToggleDoor();
            }
            else
            {
                ResetInteraction();
            }
        }
        else
        {
            ToggleDoor();
        }
    }

    protected virtual void ToggleDoor()
    {
        if(_isLock || _isAnimating) return;
        
        _isOpen = !_isOpen;
        
        switch (doorType)
        {
            case DoorType.Horizontal:
                StartCoroutine(AnimateHorizontal());
                break;
            case DoorType.Vertical:
                StartCoroutine(AnimateVertical());
                break;
            case DoorType.Rotate:
                StartCoroutine(AnimateRotate());
                break;
            case DoorType.Brake:
                StartCoroutine(AnimateExplosion());
                break;
        }
        
        interactableText = _isOpen ? "문 닫기" : "문 열기";
    }
    
    // 가로 미닫이 문
    private IEnumerator AnimateHorizontal()
    {
        _isAnimating = true;
        
        Vector3 leftStartPos = Vector3.zero;
        Vector3 rightStartPos = Vector3.zero;
        Vector3 leftTargetPos = Vector3.zero;
        Vector3 rightTargetPos = Vector3.zero;
        
        // 왼쪽 문 설정
        if (leftDoor != null)
        {
            leftStartPos = leftDoor.localPosition;
            leftTargetPos = _isOpen ? 
                _leftDoorInitialPosition + Vector3.left * slideDistance : 
                _leftDoorInitialPosition;
        }
        
        // 오른쪽 문 설정
        if (rightDoor != null)
        {
            rightStartPos = rightDoor.localPosition;
            rightTargetPos = _isOpen ? 
                _rightDoorInitialPosition + Vector3.right * slideDistance : 
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
        
        // 최종 위치 설정
        if (leftDoor != null)
            leftDoor.localPosition = leftTargetPos;
        
        if (rightDoor != null)
            rightDoor.localPosition = rightTargetPos;
        
        _isAnimating = false;
        ResetInteraction();
    }
    
    // 세로 미닫이 문
    private IEnumerator AnimateVertical()
    {
        _isAnimating = true;
        
        Vector3 leftStartPos = Vector3.zero;
        Vector3 rightStartPos = Vector3.zero;
        Vector3 leftTargetPos = Vector3.zero;
        Vector3 rightTargetPos = Vector3.zero;
        
        // 위쪽 문 설정 (leftDoor를 위쪽으로 사용)
        if (leftDoor != null)
        {
            leftStartPos = leftDoor.localPosition;
            leftTargetPos = _isOpen ? 
                _leftDoorInitialPosition + Vector3.up * slideDistance : 
                _leftDoorInitialPosition;
        }
        
        // 아래쪽 문 설정 (rightDoor를 아래쪽으로 사용)
        if (rightDoor != null)
        {
            rightStartPos = rightDoor.localPosition;
            rightTargetPos = _isOpen ? 
                _rightDoorInitialPosition + Vector3.down * slideDistance : 
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
        
        // 최종 위치 설정
        if (leftDoor != null)
            leftDoor.localPosition = leftTargetPos;
        
        if (rightDoor != null)
            rightDoor.localPosition = rightTargetPos;
        
        _isAnimating = false;
        ResetInteraction();
    }
    
    // 회전하는 문
    private IEnumerator AnimateRotate()
    {
        _isAnimating = true;
        
        Quaternion leftStartRotation = Quaternion.identity;
        Quaternion rightStartRotation = Quaternion.identity;
        Quaternion leftTargetRotation = Quaternion.identity;
        Quaternion rightTargetRotation = Quaternion.identity;
        
        // 왼쪽 문 설정
        if (leftDoor != null)
        {
            leftStartRotation = leftDoor.localRotation;
            leftTargetRotation = _isOpen ? 
                _leftDoorInitialRotation * Quaternion.Euler(0, -rotationAngle, 0) : 
                _leftDoorInitialRotation;
        }
        
        // 오른쪽 문 설정
        if (rightDoor != null)
        {
            rightStartRotation = rightDoor.localRotation;
            rightTargetRotation = _isOpen ? 
                _rightDoorInitialRotation * Quaternion.Euler(0, rotationAngle, 0) : 
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
        
        // 최종 회전 설정
        if (leftDoor != null)
            leftDoor.localRotation = leftTargetRotation;
        
        if (rightDoor != null)
            rightDoor.localRotation = rightTargetRotation;
        
        _isAnimating = false;
        ResetInteraction();
    }

    private IEnumerator AnimateExplosion()
    {
        _isAnimating = true;
        
        effectPlayer.PlayAllParticles();
        yield return new WaitForSeconds(1f);
        if (leftDoor != null)
        {
            leftDoor.gameObject.SetActive(false);
        }
        
        // 오른쪽 문 설정
        if (rightDoor != null)
        {
            rightDoor.gameObject.SetActive(false);
        }
        
    }
}