using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take ActionPoint Damage")]
public class TakeStaminaEffect : IInstantCharacterEffect
{
    public int actionCost;
    public override void ProcessEffect(CharacterManager effectTarget)
    {
        CalculateActionPointDamage(effectTarget);
    }

    private void CalculateActionPointDamage(CharacterManager character)
    {
        character.characterVariableManager.stamina.Value -= actionCost;
    }
}
