using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class MapInfoTrigger : MonoBehaviour
{
   [SerializeField] private GameObject vCam;
   
   [SerializeField] private GameObject canvasUI;
   [SerializeField] private TextMeshProUGUI mapInfoText;
   [SerializeField] private string mapTitle;
   
   private bool _isTrigger = false;
   
   private void Start()
   {
      _isTrigger = false;
      vCam.SetActive(false);
      canvasUI.SetActive(false);
   }
   
   private void OnTriggerEnter(Collider other)
   {
      // 플레이어가 트리거 영역에 들어오고, 아직 튜토리얼이 실행되지 않았을 때
      if (other.CompareTag("Player") && !_isTrigger)
      {
         StartCoroutine(PlayTutorialSequence());
         _isTrigger = true;
      }
   }
   
   private IEnumerator PlayTutorialSequence()
   {
      // 플레이어 제어 비활성화
      PlayerInputManager.Instance.SetControlActive(false);
      GUIController.Instance.playerUIHudManager.DeactivateHUD();
      vCam.SetActive(true);
        
      // 잠시 대기
      yield return new WaitForSeconds(1.5f);
      
      canvasUI.SetActive(true);
            
      mapInfoText.text = mapTitle;
      
      // 튜토리얼 시간 동안 대기
      yield return new WaitForSeconds(1.5f);
      
      canvasUI.SetActive(false);
      
      vCam.SetActive(false);
            
      // 플레이어 제어 다시 활성화
      PlayerInputManager.Instance.SetControlActive(true);
      GUIController.Instance.playerUIHudManager.ActiveHUD();
   }
}
