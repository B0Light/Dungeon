using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CreditItem
{
    public string role;           // 역할 (예: "프로그래머", "아티스트")
    public string name;           // 이름
    public bool isTitle;          // 제목인지 여부
    public bool isSpacing;        // 빈 공간인지 여부
    public float customSpacing;   // 커스텀 간격
    
    public CreditItem(string role, string name, bool isTitle = false, bool isSpacing = false, float customSpacing = 0f)
    {
        this.role = role;
        this.name = name;
        this.isTitle = isTitle;
        this.isSpacing = isSpacing;
        this.customSpacing = customSpacing;
    }
}

public class GameCredits : MonoBehaviour
{
    [Header("UI References")]
    public Transform creditsContainer;
    public GameObject creditTextPrefab;
    public ScrollRect scrollRect;
    public Button skipButton;
    
    [Header("Credit Settings")]
    public float scrollSpeed = 50f;
    public float titleFontSize = 36f;
    public float roleFontSize = 24f;
    public float nameFontSize = 20f;
    public Color titleColor = Color.white;
    public Color roleColor = Color.yellow;
    public Color nameColor = Color.white;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    public bool autoStart = true;
    public bool loopCredits = false;
    
    [Header("Audio")]
    public AudioSource creditsMusic;
    
    private List<CreditItem> creditsList = new List<CreditItem>();
    private List<GameObject> creditObjects = new List<GameObject>();
    private bool isPlaying = false;
    private Coroutine scrollCoroutine;
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        SetupCredits();
        
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCredits);
    }
    
    void SetupCredits()
    {
        // 크레딧 데이터 설정 (예시)
        creditsList.Clear();
        
        // 게임 제목
        creditsList.Add(new CreditItem("", "STEAL ALIVE", true));
        creditsList.Add(new CreditItem("", "", false, true, 100f));
        
        // 개발팀
        //creditsList.Add(new CreditItem("", "개발팀", true));
        //creditsList.Add(new CreditItem("", "", false, true, 50f));
        creditsList.Add(new CreditItem("기획 및 제작", "장보광"));
        //creditsList.Add(new CreditItem("프로그래머", "장보광"));
        //creditsList.Add(new CreditItem("", "", false, true, 30f));
        
        //creditsList.Add(new CreditItem("아티스트", "장보광"));
        //creditsList.Add(new CreditItem("", "", false, true, 30f));
        
        //creditsList.Add(new CreditItem("사운드 디자이너", "장보광"));
        //creditsList.Add(new CreditItem("", "", false, true, 50f));
        
        // 특별 감사
        creditsList.Add(new CreditItem("", "Special Thanks", true));
        creditsList.Add(new CreditItem("", "", false, true, 50f));
        creditsList.Add(new CreditItem("베타 테스터", "파스텔"));
        creditsList.Add(new CreditItem("", "", false, true, 100f));
        
        creditsList.Add(new CreditItem("", "Thank you for playing!", true));
    }
    
    public void StartCredits()
    {
        if (isPlaying) return;
        
        isPlaying = true;
        CreateCreditObjects();
        
        if (creditsMusic != null)
            creditsMusic.Play();
            
        scrollCoroutine = StartCoroutine(ScrollCredits());
        StartCoroutine(FadeIn());
    }
    
    void CreateCreditObjects()
    {
        // 기존 오브젝트 제거
        foreach (GameObject obj in creditObjects)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        creditObjects.Clear();
        
        foreach (CreditItem credit in creditsList)
        {
            if (credit.isSpacing)
            {
                // 빈 공간 생성
                GameObject spacer = new GameObject("Spacer");
                spacer.transform.SetParent(creditsContainer);
                
                RectTransform rect = spacer.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(0, credit.customSpacing);
                
                creditObjects.Add(spacer);
                continue;
            }
            
            GameObject creditObj = Instantiate(creditTextPrefab, creditsContainer);
            TextMeshProUGUI textComponent = creditObj.GetComponent<TextMeshProUGUI>();
            
            if (textComponent == null)
                textComponent = creditObj.AddComponent<TextMeshProUGUI>();
            
            // 텍스트 설정
            if (credit.isTitle)
            {
                textComponent.text = credit.name;
                textComponent.fontSize = titleFontSize;
                textComponent.color = titleColor;
                textComponent.fontStyle = FontStyles.Bold;
            }
            else
            {
                if (!string.IsNullOrEmpty(credit.role))
                {
                    textComponent.text = $"<color=#{ColorUtility.ToHtmlStringRGB(roleColor)}>{credit.role}</color>\n{credit.name}";
                    textComponent.fontSize = nameFontSize;
                }
                else
                {
                    textComponent.text = credit.name;
                    textComponent.fontSize = nameFontSize;
                }
                textComponent.color = nameColor;
            }
            
            textComponent.alignment = TextAlignmentOptions.Center;
            creditObjects.Add(creditObj);
        }
        
        // 레이아웃 새로고침
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsContainer.GetComponent<RectTransform>());
    }
    
    IEnumerator ScrollCredits()
    {
        if (scrollRect == null) yield break;
    
        scrollRect.verticalNormalizedPosition = 1f; // 맨 위부터 시작
    
        while (scrollRect.verticalNormalizedPosition > 0f)
        {
            scrollRect.verticalNormalizedPosition -= (scrollSpeed / 1000f) * Time.deltaTime;
            yield return null;
        }
    
        // 크레딧 끝
        yield return new WaitForSeconds(2f);
    
        if (loopCredits)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            scrollCoroutine = StartCoroutine(ScrollCredits());
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }
    
    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        EndCredits();
    }
    
    public void SkipCredits()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        
        StartCoroutine(FadeOut());
    }
    
    void EndCredits()
    {
        isPlaying = false;
        
        if (creditsMusic != null)
            creditsMusic.Stop();
            
        // 크레딧 종료 후 처리 (예: 메인 메뉴로 돌아가기)
        OnCreditsComplete();
    }
    
    protected virtual void OnCreditsComplete()
    {
        // 오버라이드하여 크레딧 완료 후 동작 구현
        Debug.Log("크레딧이 완료되었습니다!");
        
        // 예시: 메인 메뉴로 돌아가기
        DeactivateCreditPanel();
    }
    
    // 런타임에서 크레딧 데이터 변경
    public void SetCustomCredits(List<CreditItem> customCredits)
    {
        creditsList = new List<CreditItem>(customCredits);
    }
    
    // 크레딧 일시정지/재개
    public void PauseCredits()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        
        if (creditsMusic != null)
            creditsMusic.Pause();
    }
    
    public void ResumeCredits()
    {
        if (isPlaying && scrollCoroutine == null)
        {
            scrollCoroutine = StartCoroutine(ScrollCredits());
            
            if (creditsMusic != null)
                creditsMusic.UnPause();
        }
    }

    public void ActivateCreditPanel()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    public void DeactivateCreditPanel()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}