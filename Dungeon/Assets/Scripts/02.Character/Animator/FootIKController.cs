using UnityEngine;

public class FootIKController : MonoBehaviour
{
    private Animator animator;
    
    // IK 관련 변수들
    [Range(0, 1)] public float rightFootWeight = 1.0f;
    [Range(0, 1)] public float leftFootWeight = 1.0f;
    public bool enableIK = true;
    
    // 레이캐스트 관련 변수들
    public LayerMask groundLayer;
    public float raycastDistance = 1.5f;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || !enableIK) return;
        
        // 발의 IK 적용
        ApplyFootIK(AvatarIKGoal.RightFoot, rightFootWeight);
        ApplyFootIK(AvatarIKGoal.LeftFoot, leftFootWeight);
    }
    
    void ApplyFootIK(AvatarIKGoal foot, float weight)
    {
        // 현재 발의 위치와 회전값 가져오기
        Vector3 footPosition = animator.GetIKPosition(foot);
        Quaternion footRotation = animator.GetIKRotation(foot);
        
        // 발 위치에서 아래 방향으로 레이를 쏘아 지면 감지
        RaycastHit hit;
        Ray ray = new Ray(footPosition + Vector3.up * 0.5f, Vector3.down);
        
        if (Physics.Raycast(ray, out hit, raycastDistance, groundLayer))
        {
            // 지면에 발을 위치시키기
            footPosition.y = hit.point.y;
            
            // 지면의 법선 벡터에 맞춰 발 회전 조정
            footRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * footRotation;
            
            // IK 목표 설정
            animator.SetIKPositionWeight(foot, weight);
            animator.SetIKPosition(foot, footPosition);
            
            animator.SetIKRotationWeight(foot, weight);
            animator.SetIKRotation(foot, footRotation);
        }
    }
}