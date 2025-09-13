
public class InteractablePerkController : Interactable
{
    public WireframeShader wireframeShader;
    

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        GUIController.Instance.OpenPerkManager(player, this);
    }
}
