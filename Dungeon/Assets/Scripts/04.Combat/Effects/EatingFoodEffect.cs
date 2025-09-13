using UnityEngine;

public class EatingFoodEffect : IInstantCharacterEffect
{
    private int _immediateHealAmount;
    
    public void SetFoodAmount(int immediateHealAmount)
    {
        _immediateHealAmount = immediateHealAmount;
    }
    
    public override void ProcessEffect(CharacterManager effectTarget)
    {
        if(effectTarget.isDead.Value) return;
        
        Instantiate(WorldCharacterEffectsManager.Instance.healVFX, effectTarget.transform.position + Vector3.up, Quaternion.identity);
        EatingProcess(effectTarget);
    }
    
    private void EatingProcess(CharacterManager character)
    {
        if(character is PlayerManager playerManager)
            playerManager.playerVariableManager.hungerLevel.Value += _immediateHealAmount;
    }
}
