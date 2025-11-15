
public class InteractableBuildController : Interactable
{
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        EnterController();
    }

    private void EnterController()
    {
        GUIController.Instance.ToggleMainGUI(false);
        InputHandlerManager.Instance.SetInputMode(InputMode.OpenUI);
        ShelterBuildHUDManager.Instance.ToggleMainBuildHUD(true, this);
    }
}
