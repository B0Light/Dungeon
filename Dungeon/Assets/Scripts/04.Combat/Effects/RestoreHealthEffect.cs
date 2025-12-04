using UnityEngine;
using System.Collections;
public class RestoreHealthEffect : IInstantEffect
{
    private int _immediateHealAmount;
    private int _continuousHealAmount;
    private int _duration;

    public void SetHealAmount(int immediateHealAmount, int continuousHealAmount, int duration)
    {
        _immediateHealAmount = immediateHealAmount;
        _continuousHealAmount = continuousHealAmount;
        _duration = duration;
    }
    public override void ProcessEffect(IEffectable effectTarget)
    {
        if (effectTarget is CharacterManager characterManager)
        {
            if (characterManager.isDead.Value) return;

            Instantiate(WorldCharacterEffectsManager.Instance.healVFX, characterManager.transform.position + Vector3.up,
                Quaternion.identity);
            HealProcess(characterManager);
            characterManager.StartCoroutine(HealOverTime(characterManager));
        }
    }

    private void HealProcess(CharacterManager character)
    {
        character.characterVariableManager.health.Value += _immediateHealAmount;
    }
    
    private IEnumerator HealOverTime(CharacterManager character)
    {
        int elapsedTime = 0;

        while (elapsedTime <= _duration)
        {
            character.characterVariableManager.health.Value += _continuousHealAmount;
            
            yield return new WaitForSeconds(1f);

            elapsedTime += 1;
        }
    }
}
