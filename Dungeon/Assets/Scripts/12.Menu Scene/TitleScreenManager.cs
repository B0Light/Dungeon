using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class TitleScreenManager : MonoBehaviour
{
    public static TitleScreenManager Instance; 
    [Header("Menus")]
    [SerializeField] private GameObject titleScreenMainMenu;
    [SerializeField] private GameObject titleScreenLoadMenu;
    [SerializeField] private GameCredits titleScreenCredit;

    [Header("Pop Ups")] 
    [SerializeField] private GameObject noCharacterSlotsPopUp;
    [SerializeField] private Button noCharacterSlotsOkayButton;
    [SerializeField] private GameObject deleteCharacterSlotPopUp;
    [SerializeField] private GameObject deleteAllSlotPopUp;
    [SerializeField] private GameObject inputPlayerNamePopUp;
    [SerializeField] private TMP_InputField playerNameInputField;
    
    private TitleScreenLoadMenuInputManager _loadMenuInputManager;

    [Header("Character Slots")] 
    private CharacterSlot _currentSelectedSlot = CharacterSlot.NO_SLOT;
    private event Action OnSelectSlotChange;
    public CharacterSlot CurrentSelectedSlot
    {
        get => _currentSelectedSlot;
        private set
        {
            // event발생 
            if (_currentSelectedSlot == value) return;
            
            _currentSelectedSlot = value;
            OnSelectSlotChange?.Invoke();
        }
    }

    private void OnEnable()
    {
        _loadMenuInputManager = titleScreenLoadMenu.GetComponent<TitleScreenLoadMenuInputManager>();
        OnSelectSlotChange += _loadMenuInputManager.SetSlot;
    }

    private void OnDisable()
    {
        OnSelectSlotChange -= _loadMenuInputManager.SetSlot;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleInputPlayerNamePopUp(bool value)
    {
        inputPlayerNamePopUp.SetActive(value);
    }
    
    public void SetPlayerNameAtInputField()
    {
        string userInput = playerNameInputField.text;
        if (userInput == "")
        {
            userInput = "Player";
        }
        StartNewGame(userInput);
    }
    
    private void StartNewGame(string playerName)
    {
        WorldSaveGameManager.Instance.AttemptToCreateNewGame(playerName);
        UIManager.Instance.MouseActive(false);
    }

    public void ContinueLastGame()
    {
        if (!WorldSaveGameManager.Instance.LoadLastGame())
        {
            // 저장 데이터 없음 - 게임 실행 실패 
            ToggleInputPlayerNamePopUp(true);
        }
        else
        {
            // 게임 정상 시작 
            UIManager.Instance.MouseActive(false);
        }
    }

    public void OpenLoadGameMenu()
    {
        // CLOSE MAIN MENU
        titleScreenMainMenu.SetActive(false);

        // OPEN LOAD MENU
        titleScreenLoadMenu.SetActive(true);
    }

    public void CloseLoadGameMenu()
    {
        titleScreenLoadMenu.SetActive(false);
        titleScreenMainMenu.SetActive(true);
    }

    public void OpenCredit()
    {
        titleScreenLoadMenu.SetActive(false);
        titleScreenCredit.ActivateCreditPanel();
        
        titleScreenCredit.StartCredits();
    }
    
    public void CloseCredit()
    {
        titleScreenCredit.DeactivateCreditPanel();
        
        titleScreenMainMenu.SetActive(true);
        titleScreenLoadMenu.SetActive(false);
        
    }
    
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중이면 중지
#else
            Application.Quit(); // 빌드된 게임에서는 종료
#endif
    }

    public void OpenNoFreeCharacterSlotsPopUp()
    {
        noCharacterSlotsPopUp.SetActive(true);
        noCharacterSlotsOkayButton.Select();
    }

    public void CloseNoFreeCharactersSlotsPopUp()
    {
        noCharacterSlotsPopUp.SetActive(false);
    }

    // CHARACTER SLOTS  

    public void SelectCharacterSlot(CharacterSlot characterSlot)
    {
        CurrentSelectedSlot = characterSlot;
    }

    public void SelectNoSlot()
    {
        CurrentSelectedSlot = CharacterSlot.NO_SLOT;
    }

    public void OpenToDeleteCharacterSlot()
    {
        if (CurrentSelectedSlot != CharacterSlot.NO_SLOT)
        {
            deleteCharacterSlotPopUp.SetActive(true);
        }
    }
    
    public void CloseDeleteSlotMenu()
    {
        deleteCharacterSlotPopUp.SetActive(false);
    }

    public void DeleteCharacterSlot()
    {
        deleteCharacterSlotPopUp.SetActive(false);
        WorldSaveGameManager.Instance.DeleteGame(CurrentSelectedSlot);

        // WE REFRESH THE SLOTS AFTER DELETION
        titleScreenLoadMenu.SetActive(false);
        titleScreenLoadMenu.SetActive(true);
    }
    
    public void OpenToDeleteAllSlot()
    {
        deleteAllSlotPopUp.SetActive(true);
    }
    
    public void CloseDeleteAllSlotMenu()
    {
        deleteAllSlotPopUp.SetActive(false);
    }

    public void DeleteAllCharacterSlot()
    {
        WorldSaveGameManager.Instance.DeleteAllGame();
        
        CloseDeleteAllSlotMenu();
        titleScreenLoadMenu.SetActive(false);
        titleScreenMainMenu.SetActive(true);
    }
}

