using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractableNpc : Interactable
{
    [SerializeField] protected string npcName = "";
    [SerializeField] protected string interactionMsg = "";
    
    [Header("VCam")] 
    [SerializeField] protected CinemachineVirtualCameraBase vCam;
    
    private void Start()
    {
        vCam.Priority = 0;
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);

        vCam.Priority = 20;
        GUIController.Instance.OpenDialogue(npcName, ResetInteraction);
        GUIController.Instance.dialogueGUIManager.SetDialogueText(interactionMsg);
    }
    
    public override void ResetInteraction()
    { 
        Debug.LogWarning("Reset Interaction");
        vCam.Priority = 0;
        
        PlayerInputManager.Instance.SetControlActive(true);
        
        base.ResetInteraction();
    }
}
