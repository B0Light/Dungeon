using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take ActionPoint Damage")]
public class TakeStaminaEffect : IInstantEffect
{
    public int actionCost;
    public override void ProcessEffect(IEffectable effectTarget)
    {
        if (effectTarget is CharacterManager characterManager)
        {
            CalculateActionPointDamage(characterManager);
        }
    }

    private void CalculateActionPointDamage(CharacterManager character)
    {
        character.characterVariableManager.stamina.Value -= actionCost;
    }
}
