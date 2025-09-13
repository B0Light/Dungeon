using System.Collections;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Serialization;

public class UI_InteractionCountDown : MonoBehaviour
{
    
    public float interactionTime = 2.0f;
    
    [Header("Interaction Obj")]
    [SerializeField] private GameObject textSlot;
    [SerializeField] private TMP_Text timerText;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        Toggle(false);
    }

    public void Interaction(Action callback)
    {
        StartCoroutine(WaitForInteraction(callback));
    }
    
    private IEnumerator WaitForInteraction(Action callback)
    {
        Toggle(true);
        
        float elapsedTime = 0.0f;

        while (elapsedTime < interactionTime)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = (interactionTime - elapsedTime).ToString("F2") + " seconds remaining";
            yield return null;
        }

        timerText.text = "";
        Toggle(false);
        callback?.Invoke();
    }

    private void Toggle(bool isActive)
    {
        _canvasGroup.alpha = isActive ? 1 : 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
