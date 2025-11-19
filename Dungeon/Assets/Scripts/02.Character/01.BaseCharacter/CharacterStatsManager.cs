using System;
using UnityEngine;

public class CharacterStatsManager : MonoBehaviour
{
    private CharacterManager _character;

    private readonly float _staminaRegenDelayTime = 1.0f; // 재생이 시작되기까지의 딜레이 시간 (초)
    private readonly float _staminaRegenRatePerSecond = 15.0f; // 초당 재생되는 스태미나 값
    private readonly float _staminaDrainRatePerSecond = 10.0f; // 초당 감소하는 스태미나 값 (달리기 시)
    private float _staminaRegenTimer = 0.0f; // 재생 딜레이를 위한 타이머
    
    public Variable<int> extraDamage = new Variable<int>(0);
    
    [Header("Base Absorptions percent")] 
    // 피해 경감
    [Range(0,10)] public float basePhysicalAbsorption = 0;
    [Range(0,10)] public float baseMagicalAbsorption = 0;
    
    public float extraPhysicalAbsorption = 0;
    public float extraMagicalAbsorption = 0;

    [Header("Blocking Absorptions percent")] 
    // 가드시 피해 경감 
    [Range(0,100)] public float blockingPhysicalAbsorption = 0;
    [Range(0,100)] public float blockingMagicalAbsorption = 0;
    
    protected virtual void Awake()
    {
        _character = GetComponent<CharacterManager>();
    }

    private void Update()
    {
        RegenerateStamina();
    }

    private void RegenerateStamina()
    {
        if (_character.characterVariableManager.stamina.Value <= 0)
        {
            _character.characterVariableManager.stamina.Value = 0;
            _character.characterVariableManager.CLVM.isSprinting = false;
        }
        
        // 1. 달리기 시 스테미나 감소
        if (_character.characterVariableManager.CLVM.isSprinting && _character.characterVariableManager.CLVM.speed2D > 1f)
        {
            _staminaRegenTimer = 0.0f;

            _character.characterVariableManager.stamina.Value -=
                _staminaDrainRatePerSecond * Time.deltaTime;

            return;
        }
        
        // 2. 이미 최대인 경우 
        if (_character.characterVariableManager.stamina.Value >=
            _character.characterVariableManager.stamina.MaxValue)
        {
            _character.characterVariableManager.stamina.Value =
                _character.characterVariableManager.stamina.MaxValue;
        
            _staminaRegenTimer = 0.0f; 
        
            return;
        }

        // 3. 행동 중일 때 재생 중단
        if (_character.isPerformingAction)
        {
            // 딜레이 타이머 리셋 (행동 중에는 재생 딜레이도 초기화)
            _staminaRegenTimer = 0.0f;
            return;
        }

        // 4. 비활동 (가만히 있는) 상태: 재생 딜레이 타이머 증가
        _staminaRegenTimer += Time.deltaTime;

        // 5. 딜레이 시간(RegenDelayTime)이 지난 후, 스테미나 재생 시작
        if (_staminaRegenTimer >= _staminaRegenDelayTime)
        {
            _character.characterVariableManager.stamina.Value +=
                _staminaRegenRatePerSecond * Time.deltaTime;

            if (_character.characterVariableManager.stamina.Value > _character.characterVariableManager.stamina.MaxValue)
            {
                _character.characterVariableManager.stamina.Value =
                    _character.characterVariableManager.stamina.MaxValue;
            }
        }
    }
    
    public bool UseStamina(float value = 10)
    {
        if (_character.characterVariableManager.stamina.Value < value) return false;
        
        _character.characterVariableManager.stamina.Value -= value;
        return true;
    }
}

