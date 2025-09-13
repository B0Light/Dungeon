using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIState : ScriptableObject
{
    public virtual void OnEnterState(AICharacterManager aiCharacter)
    {
        // 기본 구현 (필요 시 공통 초기화 로직 추가 가능)
    }
    public virtual AIState Tick(AICharacterManager aiCharacter)
    {
        //Debug.Log("CURRENT STATE" + this.name);
        return this;
    }

    protected AIState SwitchState(AICharacterManager aiCharacter, AIState newState)
    {
        ResetStateFlags(aiCharacter);
        return newState;
    }

    protected virtual void ResetStateFlags(AICharacterManager aiCharacter)
    {
        
    }
}
