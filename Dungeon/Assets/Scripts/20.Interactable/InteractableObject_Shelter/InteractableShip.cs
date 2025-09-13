using UnityEngine;

public class InteractableShip : Interactable
{
    [SerializeField] private GameObject interactionMark;

    [SerializeField] private string mapName = "";

    private void Start()
    {
        interactionMark.SetActive(true);
    }

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        OnBoard();
    }

    private void OnBoard()
    {
        interactionMark.SetActive(false);
        WorldSceneChangeManager.Instance.LoadSceneAsync(mapName);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
