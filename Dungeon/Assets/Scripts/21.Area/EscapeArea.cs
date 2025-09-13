using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class EscapeArea : Area
{
   [SerializeField] private float escapeTime = 10f;
    
   [Header("Interaction Obj")]
   [SerializeField] private GameObject textSlot;
   [SerializeField] private TMP_Text timerText;

   private CharacterManager _characterManager;
   
   private void Start()
   {
      textSlot.SetActive(false);
   }

   protected override void EnterArea(CharacterManager character)
   {
      textSlot.SetActive(true);
      StartCoroutine(WaitForInteraction(ExtractionSummary));
      _characterManager = character;
   }

   protected override void ExitArea(CharacterManager character)
   {
      textSlot.SetActive(false);
      StopAllCoroutines();
      _characterManager = null;
   }

   private void ExtractionSummary()
   {
      GUIController.Instance.OpenExtractionSummery();
      if (_characterManager != null)
      {
         _characterManager.characterVariableManager.isInvulnerable.Value = true;
      }
   }
   
   private IEnumerator WaitForInteraction(Action callback)
   {
      textSlot.SetActive(true);
        
      float elapsedTime = 0.0f;

      while (elapsedTime < escapeTime)
      {
         elapsedTime += Time.deltaTime;
         timerText.text = "Time Remaining for Extraction : " + (escapeTime - elapsedTime).ToString("F2");
         yield return null;
      }

      timerText.text = "";
      textSlot.SetActive(false);
      callback?.Invoke();
   }
}
