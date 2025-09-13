using System.Collections;
using UnityEngine;

public class InteractableArtefact : Interactable
{
    private bool _isReady = true;
    [SerializeField] private int buffTime = 60;

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        if(!_isReady) return;

        StartCoroutine(GetEffect(player));
    }
    
    private IEnumerator GetEffect(PlayerManager playerPerformingAction)
    {
        _isReady = false;
        playerPerformingAction.playerStatsManager.extraDamage.Value += 100;
        yield return new WaitForSeconds(buffTime);
        playerPerformingAction.playerStatsManager.extraDamage.Value -= 100;
        _isReady = true;
        ResetInteraction();
    }
}
