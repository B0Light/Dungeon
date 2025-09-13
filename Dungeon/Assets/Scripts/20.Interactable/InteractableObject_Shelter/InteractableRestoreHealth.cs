using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractableRestoreHealth : Interactable
{
    [SerializeField] private int immediateHealAmount;
    [SerializeField] private int continuousHealAmount;
    [SerializeField] private int duration;

    public override void Interact(PlayerManager player)
    {
        RestoreHealthEffect restoreHealthEffect =
            Instantiate(WorldCharacterEffectsManager.Instance.restoreHealthEffect);
        
        restoreHealthEffect.SetHealAmount(
            immediateHealAmount,
            continuousHealAmount,
            duration
        );
        player.characterEffectsManager.ProcessInstantEffect(restoreHealthEffect);
        
        ResetInteraction();
    }

    public override void SetToSpecificLevel(int level)
    {
        immediateHealAmount = level * 50;
        continuousHealAmount = level * 10;
        duration = level * 5;
    }
}
