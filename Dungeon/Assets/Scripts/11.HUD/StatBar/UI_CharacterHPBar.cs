using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterHPBar : UI_StatBar
{
    private CharacterManager character;
    private AICharacterManager aiCharacter;

    [SerializeField] bool displayCharacterNameOnDamage = false;
    [SerializeField] float defaultTimeBeforeBarHides = 3;
    [SerializeField] float hideTimer = 0;
    [SerializeField] int currentDamageTaken = 0;
    [SerializeField] TextMeshProUGUI characterName;
    [HideInInspector] public int oldHealthValue = 0;
    
    [Header("체력바 색상 설정")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color emptyColor = Color.gray;
    
    [Header("사망 관련 설정")]
    [SerializeField] private float deathDisplayTime = 5f; // 사망 시 체력바 표시 시간
    [SerializeField] private bool hideBarOnDeath = false; // 사망 시 체력바를 즉시 숨길지 여부
    [Header("그라데이션 설정")]
    [SerializeField] private bool useGradientTransition = true;
    [SerializeField] private float gradientWidth = 0.1f; // 그라데이션 영역의 폭
    private int _currentHealthValue;
    private int _maxHealthValue;
    protected void Awake()
    {
        character = GetComponentInParent<CharacterManager>();

        if (character != null)
        {
            aiCharacter = character as AICharacterManager;
        }
    }

    private void Start()
    {
        oldHealthValue = character.characterVariableManager.health.Value;
        _maxHealthValue = character.characterVariableManager.health.MaxValue;
        _currentHealthValue = oldHealthValue;
        UpdateHealthBar();
        
        gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);

        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        currentDamageTaken = 0;
        // 비활성화될 때 현재 체력 상태를 저장
        if (character != null)
        {
            oldHealthValue = character.characterVariableManager.health.Value;
            _currentHealthValue = character.characterVariableManager.health.Value;
        }
    }

    public override void SetStat(int newValue)
    {
        if (displayCharacterNameOnDamage)
        {
            characterName.enabled = true;

            if (aiCharacter != null)
            {
                Debug.Log("CHARACTER NAME : " + aiCharacter.characterName);
                characterName.text = aiCharacter.characterName;
            }
            else
            {
                Debug.Log("NO AI CHARACTER");
            }
        }

        //  CALL THIS HERE INCASE MAX HEALTH CHANGES FROM A CHARACTER EFFECT/BUFF ETC...
        _maxHealthValue = character.characterVariableManager.health.MaxValue;
        _currentHealthValue = newValue;

        //  TO DO: RUN SECONDARY BAR LOGIC (YELLOW BAR THAT APPEARS BEHIND HP WHEN DAMAGED)

        //  TOTAL THE DAMAGE TAKEN WHILST THE BAR IS ACTIVE
        currentDamageTaken = Mathf.RoundToInt(oldHealthValue - newValue);

        if (currentDamageTaken < 0)
        {
            currentDamageTaken = Mathf.Abs(currentDamageTaken);
        }

        // 텍스트 그라디언트 HP바 업데이트
        UpdateHealthBar();

        // 체력이 0이 되면 사망 처리
        if (newValue <= 0)
        {
            if (hideBarOnDeath)
            {
                // 사망 시 즉시 체력바 숨김
                gameObject.SetActive(false);
            }
            else
            {
                // 사망 시 체력바를 지정된 시간만큼 표시
                hideTimer = deathDisplayTime;
                gameObject.SetActive(true);
            }
        }
        else if (character.characterVariableManager.health.Value != character.characterVariableManager.health.MaxValue)
        {
            hideTimer = defaultTimeBeforeBarHides;
            gameObject.SetActive(true);
        }
    }

    public override void SetMaxStat(int maxValue)
    {
        _maxHealthValue = maxValue;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (characterName == null) return;

        characterName.ForceMeshUpdate();
        TMP_TextInfo textInfo = characterName.textInfo;
        
        // 체력이 0 이하일 때는 명확히 0%로 처리
        float healthPercent = _maxHealthValue > 0 ? Mathf.Max(0f, (float)_currentHealthValue / _maxHealthValue) : 0f;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            float charPosition = (float)i / (textInfo.characterCount - 1);
            Color charColor;
            
            // 체력이 0일 때는 모든 글자를 emptyColor로 설정
            if (_currentHealthValue <= 0)
            {
                charColor = emptyColor;
            }
            else if (useGradientTransition)
            {
                // 그라데이션 전환
                if (charPosition < healthPercent - gradientWidth)
                {
                    // 완전히 채워진 부분
                    charColor = GetHealthColor(healthPercent);
                }
                else if (charPosition <= healthPercent + gradientWidth)
                {
                    // 그라데이션 부분
                    float gradientFactor = (healthPercent + gradientWidth - charPosition) / (2 * gradientWidth);
                    gradientFactor = Mathf.Clamp01(gradientFactor);
                    charColor = Color.Lerp(emptyColor, GetHealthColor(healthPercent), gradientFactor);
                }
                else
                {
                    // 빈 부분
                    charColor = emptyColor;
                }
            }
            else
            {
                // 단순 전환
                charColor = charPosition <= healthPercent ? GetHealthColor(healthPercent) : emptyColor;
            }
            
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            
            if (textInfo.characterInfo[i].isVisible)
            {
                for (int j = 0; j < 4; j++)
                {
                    textInfo.meshInfo[0].colors32[vertexIndex + j] = charColor;
                }
            }
        }
        
        characterName.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    
    Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > 0.6f)
            return Color.Lerp(midHealthColor, fullHealthColor, (healthPercent - 0.6f) / 0.4f);
        else if (healthPercent > 0.3f)
            return Color.Lerp(lowHealthColor, midHealthColor, (healthPercent - 0.3f) / 0.3f);
        else
            return lowHealthColor;
    }
}