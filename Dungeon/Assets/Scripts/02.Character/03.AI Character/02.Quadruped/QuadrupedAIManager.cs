using UnityEngine;
using UnityEngine.Serialization;

public class QuadrupedAIManager : AICharacterManager
{
    [HideInInspector] public QuadrupedAICombatManager quadrupedAICombatManager;
    [HideInInspector] public QuadrupedAIAnimatorManager quadrupedAIAnimatorManager;
    protected override void Awake()
    {
        base.Awake();
        quadrupedAICombatManager = aiCharacterCombatManager as QuadrupedAICombatManager;
        quadrupedAIAnimatorManager = characterAnimatorManager as QuadrupedAIAnimatorManager;
    }
}
