using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WorldSceneChangeManager : Singleton<WorldSceneChangeManager>
{
    // 로딩 바 UI를 위한 Slider
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject continueText;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    private CanvasGroup _canvasGroup;
    [SerializeField] private RandomTooltipSystem randomTooltipSystem;
    [Header("Loading Progress Control")]
    [SerializeField] private float maxProgressSpeed = 0.5f; // 초당 최대 진행 속도 (0.5 = 50%/초)
    [SerializeField] private float minProgressSpeed = 0.1f; // 초당 최소 진행 속도 (0.1 = 10%/초)
    [SerializeField] private bool useProgressSpeedLimit = true; // 속도 제한 사용 여부
    
    private float _currentDisplayProgress = 0f; // 현재 표시되는 진행률
    [SerializeField] private string titleSceneName = "01.TitleScene";
    [SerializeField] private string shelterSceneName = "03.Shelter";
    public static event Action OnSceneChanged;

    protected override void Awake()
    {
        base.Awake();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        continueText.SetActive(false);
        loadingScreen.SetActive(false);
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void LoadSceneAsync(string sceneCode)
    {
        StartCoroutine(LoadSceneCoroutine(sceneCode));
    }
    
    public void LoadShelter()
    {
        StartCoroutine(LoadSceneCoroutine(shelterSceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneToLoad)
    {
        // 씬을 정수 코드로 로드
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        yield return StartCoroutine(HandleSceneLoading(asyncOperation, sceneToLoad));
        
        OnSceneChanged?.Invoke();
    }

    private IEnumerator HandleSceneLoading(AsyncOperation asyncOperation, string sceneToLoad)
    {
        // 씬 자동 활성화 방지
        asyncOperation.allowSceneActivation = false;

        if (sceneToLoad == titleSceneName)
        {
            GUIController.Instance.playerUIHudManager.DeactivateHUD();
        }
        else
        {
            GUIController.Instance.playerUIHudManager.ActiveHUD();
        }
        
        loadingScreen.SetActive(true);
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        
        // 진행률 초기화
        _currentDisplayProgress = 0f;
        
        bool sceneLoadingCompleted = false;
        randomTooltipSystem?.StartTooltipSystem();
        
        while (!asyncOperation.isDone)
        {
            // 90%까지는 실제 로딩 진행 상황 표시 (속도 제한 적용)
            if (asyncOperation.progress < 0.9f)
            {
                if (useProgressSpeedLimit)
                {
                    // 속도 제한을 적용하여 부드럽게 진행
                    _currentDisplayProgress = UpdateProgressWithSpeedLimit(asyncOperation.progress, _currentDisplayProgress);
                }
                else
                {
                    // 속도 제한 없이 실제 진행률 사용
                    _currentDisplayProgress = asyncOperation.progress;
                }
                
                UpdateLoadingUI(_currentDisplayProgress);
            }
            
            // 90%에 도달하면 수동으로 100%까지 3초에 걸쳐 진행
            if (asyncOperation.progress >= 0.9f && !sceneLoadingCompleted)
            {
                sceneLoadingCompleted = true;
                yield return StartCoroutine(AnimateProgressTo100Percent());
                
                // 씬 활성화 허용
                asyncOperation.allowSceneActivation = true;
                yield return new WaitForSeconds(0.5f); // 씬 전환 대기
                continueText.SetActive(true);
            }
            
            yield return null;
        }
        randomTooltipSystem?.StopTooltipSystem();
    }

    /// <summary>
    /// 속도 제한을 적용하여 진행률을 업데이트
    /// </summary>
    private float UpdateProgressWithSpeedLimit(float targetProgress, float currentProgress)
    {
        float progressDifference = targetProgress - currentProgress;
        
        // 목표 진행률이 현재보다 높은 경우에만 속도 제한 적용
        if (progressDifference > 0)
        {
            float maxProgressThisFrame = maxProgressSpeed * Time.deltaTime;
            float minProgressThisFrame = minProgressSpeed * Time.deltaTime;
            
            // 진행할 양을 최대/최소 속도 범위 내로 제한
            float progressToAdd = Mathf.Clamp(progressDifference, minProgressThisFrame, maxProgressThisFrame);
            
            return currentProgress + progressToAdd;
        }
        
        return targetProgress;
    }

    /// <summary>
    /// 로딩 UI 업데이트
    /// </summary>
    private void UpdateLoadingUI(float progress)
    {
        if(loadingBar)
            loadingBar.value = progress;
        if(loadingText)
            loadingText.text = (progress * 100f).ToString("F0") + "%";
    }

    private IEnumerator AnimateProgressTo100Percent()
    {
        float duration = 3.0f; // 3초 동안 진행
        float startValue = _currentDisplayProgress; // 현재 표시 진행률에서 시작
        float endValue = 1.0f;   // 100%로 끝
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 부드러운 곡선을 위한 이징 (옵션)
            progress = Mathf.SmoothStep(0f, 1f, progress);
            
            float currentValue = Mathf.Lerp(startValue, endValue, progress);
            
            if(loadingBar)
                loadingBar.value = currentValue;
            if(loadingText)
            {
                float percentage = currentValue * 100f;
                if (percentage >= 100f)
                    loadingText.text = "Ready";
                else
                    loadingText.text = percentage.ToString("F0") + "%";
            }
            
            yield return null;
        }
        
        // 최종적으로 100% 확실히 설정
        if(loadingBar)
            loadingBar.value = 1.0f;
        if(loadingText)
            loadingText.text = "Ready";
    }

    public void LoadTitle() => LoadSceneAsync(titleSceneName);

    // Button Binding
    public void CloseLoadingScreen()
    {
        continueText.SetActive(false);
        loadingScreen.SetActive(false);
        
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}