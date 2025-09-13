using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private CinemachineVirtualCameraBase tutorialCameras; // 튜토리얼 보여줄 카메라들
    
    [Header("텍스트 설정")]
    [SerializeField] private TextMeshProUGUI tutorialText; // 튜토리얼 텍스트를 표시할 TextMeshPro
    [SerializeField] private string tutorialMessages; // 각 카메라에 대응하는 튜토리얼 메시지
    
    [Header("튜토리얼 설정")]
    [SerializeField] private float cameraSwitchDelay = 0.5f; // 카메라 전환 사이의 지연 시간
    [SerializeField] private float tutorialDuration = 3.0f; // 각 튜토리얼 카메라 지속 시간
    [SerializeField] private GameObject tutorialUI; // 튜토리얼 UI 패널
    
    private bool tutorialTriggered = false; // 튜토리얼이 이미 실행되었는지 확인
    
    private void Start()
    {
        // 시작 시 튜토리얼 UI 비활성화
        if (tutorialUI != null)
            tutorialUI.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 트리거 영역에 들어오고, 아직 튜토리얼이 실행되지 않았을 때
        if (other.CompareTag("Player") && !tutorialTriggered)
        {
            StartCoroutine(PlayTutorialSequence());
            tutorialTriggered = true;
        }
    }
    
    private IEnumerator PlayTutorialSequence()
    {
        // 플레이어 제어 비활성화
        PlayerInputManager.Instance.SetControlActive(false);
            
        // 튜토리얼 UI 활성화
        if (tutorialUI != null)
            tutorialUI.SetActive(true);
        
        // 각 튜토리얼 카메라 순차적으로 실행
        
            // 잠시 대기
            yield return new WaitForSeconds(cameraSwitchDelay);
            
            // 현재 카메라의 우선순위 올리기
            tutorialCameras.Priority = 20;
            
            // 해당 카메라에 맞는 텍스트 표시
            if (tutorialText != null)
            {
                tutorialText.text = tutorialMessages;
            }
            
            // 튜토리얼 시간 동안 대기
            yield return new WaitForSeconds(tutorialDuration);
            
            // 현재 카메라 우선순위 다시 낮추기
            tutorialCameras.Priority = 0;
        
        
        // 튜토리얼 UI 비활성화
        if (tutorialUI != null)
            tutorialUI.SetActive(false);
            
        // 플레이어 제어 다시 활성화
        PlayerInputManager.Instance.SetControlActive(true);
    }
}