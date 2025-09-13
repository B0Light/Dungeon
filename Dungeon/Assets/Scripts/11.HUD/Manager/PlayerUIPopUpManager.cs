using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerUIPopUpManager : MonoBehaviour
{
    [Header("MESSAGE POP UP")]
    [SerializeField] private GameObject popUpMessageGameObject;
    [SerializeField] private TextMeshProUGUI popUpMessageText;
    
    [Header("YOU DIED Pop Up")]
    [SerializeField] private GameObject youDiedPopUpGameObject;
    [SerializeField] private Button backToTitleButton;
    [SerializeField] private Button continueButton;
    [Header("EnemyFelled Pop Up")]
    [SerializeField] private GameObject enemyFelledPopUpGameObject;
    [SerializeField] private TextMeshProUGUI enemyFelledPopUpBackgroundText;
    [SerializeField] private TextMeshProUGUI enemyFelledPopUpText;
    [SerializeField] private CanvasGroup enemyFelledPopUpCanvasGroup;

    [Header("InteractionItem Pop UP")] 
    [SerializeField] private Image interactionItemIcon;
    [SerializeField] private CanvasGroup itemInteractionPopUpCanvasGroup;  
    [SerializeField] private TextMeshProUGUI itemInteractionPopUpText;
    
    [Header("Shop Pop Up")]
    [SerializeField] private Image shopPopUpCommentFrame;
    [SerializeField] private CanvasGroup shopPopUpCommentCanvasGroup;  
    [SerializeField] private TextMeshProUGUI shopPopUpCommentText;

    private Coroutine _curCoroutine;

    private void Start()
    {
        youDiedPopUpGameObject.SetActive(false);
        popUpMessageGameObject.SetActive(false);
    }

    public void SendYouDiedPopUp()
    {
        StartCoroutine(ShowYouDiedPopUpWithDelay());
    }

    private IEnumerator ShowYouDiedPopUpWithDelay()
    {
        yield return new WaitForSeconds(1f);
    
        PlayerInputManager.Instance.SetControlActive(false);
        youDiedPopUpGameObject.SetActive(true);
        backToTitleButton.onClick.RemoveAllListeners();
        backToTitleButton.onClick.AddListener(CloseYouDiedPopUp);
        backToTitleButton.onClick.AddListener(GameManager.Instance.HandlePostDeath_BackToTitle);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(CloseYouDiedPopUp);
        continueButton.onClick.AddListener(GameManager.Instance.HandlePostDeath_Continue);
    }

    public void CloseYouDiedPopUp()
    {
        youDiedPopUpGameObject.SetActive(false);
        PlayerInputManager.Instance.SetControlActive(true);
    }
    
    public void CloseAllPopUpWindows()
    {
        // Interactable Pop Up
        popUpMessageGameObject.SetActive(false);
        GUIController.Instance.popUpWindowIsOpen = false;
        // PickUp Pop Up
        ClosePlayerItemPickUpPopUp();
    }
    
    public void SendPlayerMessagePopUp(string messageText)
    {
        GUIController.Instance.popUpWindowIsOpen = true;
        popUpMessageText.text = messageText;
        popUpMessageGameObject.SetActive(true);
    }

    public void OpenPlayerItemPickUpPopUp(ItemInfo itemInfo)
    {
        GUIController.Instance.popUpWindowIsOpen = true;
        interactionItemIcon.sprite = itemInfo.itemIcon;
        itemInteractionPopUpText.text = itemInfo.itemName;
        itemInteractionPopUpCanvasGroup.alpha = 1;
    }
    
    private void ClosePlayerItemPickUpPopUp()
    {
        itemInteractionPopUpCanvasGroup.alpha = 0;
        itemInteractionPopUpCanvasGroup.interactable = false;
        itemInteractionPopUpCanvasGroup.blocksRaycasts = false;
    }
    
    public void SendEnemyFelledPopUp(string enemyFelledMessage)
    {
        enemyFelledPopUpText.text = enemyFelledMessage;
        enemyFelledPopUpBackgroundText.text = enemyFelledMessage;
        enemyFelledPopUpGameObject.SetActive(true);
        enemyFelledPopUpBackgroundText.characterSpacing = 0;
        StartCoroutine(ContractPopUpTextOverTime(enemyFelledPopUpBackgroundText, 5));
        StartCoroutine(FadeInPopUpOverTime(enemyFelledPopUpCanvasGroup, 5));
        StartCoroutine(WaitThenFadeOutPopUpOverTime(enemyFelledPopUpCanvasGroup, 2, 5));
    }

    public void SendShopMessagePopUp(string messageText)
    {
        shopPopUpCommentText.text = messageText;
        if(_curCoroutine != null) StopCoroutine(_curCoroutine);
        shopPopUpCommentCanvasGroup.alpha = 1;
        _curCoroutine = StartCoroutine(WaitThenFadeOutPopUpOverTime(shopPopUpCommentCanvasGroup, 1, 1));
    }
    
    
    
    private IEnumerator ContractPopUpTextOverTime(TextMeshProUGUI text, float duration)
    {
        if(duration > 0f)
        {
            text.characterSpacing = 10;
            float timer = 0;

            yield return null;

            while (duration < timer)
            {
                timer += Time.unscaledTime;
                text.characterSpacing = Mathf.Lerp(text.characterSpacing, 0, duration * (Time.unscaledTime / 20));
                yield return null;
            }
        }
    }

    private IEnumerator FadeInPopUpOverTime(CanvasGroup canvas, float duration)
    {
        if (duration > 0f)
        {
            canvas.alpha = 0;
            float timer = 0;

            yield return null;

            while(timer < duration)
            {
                timer += Time.unscaledTime;
                canvas.alpha = Mathf.Lerp(canvas.alpha,1, duration * (Time.unscaledTime));
                yield return null;
            }
        }
        canvas.alpha = 1;
        yield return null;
    }

    private IEnumerator WaitThenFadeOutPopUpOverTime(CanvasGroup canvas, float duration, float delay)
    {
        if (duration > 0f)
        {
            // 딜레이 대기
            if (delay > 0)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
        
            canvas.alpha = 1;
            float timer = 0;
            float startAlpha = canvas.alpha;

            while (timer < duration)
            {
                // Time.unscaledDeltaTime 사용 (프레임 간 경과된 실제 시간)
                float deltaTime = Time.unscaledDeltaTime;
                timer += deltaTime;
            
                // 정규화된 시간 계산 (0부터 1까지)
                float t = timer / duration;
            
                // 알파값을 1에서 0으로 부드럽게 감소
                canvas.alpha = Mathf.Lerp(startAlpha, 0, t);
            
                yield return null;
            }
        }

        // 완전히 투명하게 설정
        canvas.alpha = 0;
    
        yield return null;
        _curCoroutine = null;
    }
}
