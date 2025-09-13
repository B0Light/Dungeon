using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;


public class CharacterStatsManager : MonoBehaviour
{
    protected CharacterManager character;

    [Header("Stamina Regeneration")]
    private readonly float _actionPointRegenerationTime = 0.3f;
    private readonly float _actionPointRegenerationDelay = 0.3f;
    private float _actionPointRegenerationTimer = 0;
    private float _actionPointTickTimer = 0;
    
    public Variable<int> extraDamage = new Variable<int>(0);
    // 저항력
    public Variable<float> passivePoise = new Variable<float>(10.0f);
    
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
    
    // 가드시 자세 저항력 
    public float blockingStability = 0;
    
    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    public void RegenerateStamina()
    {
        // WE DO NOT WANT TO REGENERATE STAMINA IF WE ARE USING IT
        if (character.characterVariableManager.CLVM.isSprinting)
            return;

        if (character.isPerformingAction)
            return;

        if (character.characterVariableManager.actionPoint.Value >=
            character.characterVariableManager.actionPoint.MaxValue)
        {
            character.characterVariableManager.actionPoint.Value =
                character.characterVariableManager.actionPoint.MaxValue;
            return;
        }

        _actionPointRegenerationTimer += Time.deltaTime;

        if (_actionPointRegenerationTimer >= _actionPointRegenerationDelay)
        {
            if (character.characterVariableManager.actionPoint.Value < character.characterVariableManager.actionPoint.MaxValue)
            {
                _actionPointTickTimer += Time.deltaTime;

                if (_actionPointTickTimer >= _actionPointRegenerationTime)
                {
                    _actionPointTickTimer = 0;
                    character.characterVariableManager.actionPoint.Value += 1;
                }
            }
        }
    }
    
    public bool UseActionPoint(int value = 1)
    {
        if (character.characterVariableManager.actionPoint.Value < value) return false;
        
        character.characterVariableManager.actionPoint.Value -= value;
        _actionPointRegenerationTimer = 0;
        return true;
    }
}

