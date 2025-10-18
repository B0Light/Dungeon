using UnityEngine;

public class InputHandler_Combat : MonoBehaviour, InputHandlerManager.IInputHandler
{
    private PlayerManager _playerManager;
    private PlayerControls _playerControls;
    
    public void EnableInput()
    {
        
    }

    public void DisableInput()
    {
        
    }
    
    public void SetPlayer(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    private void OnDestroy()
    {
        if (InputHandlerManager.Instance != null)
        {
            InputHandlerManager.Instance.UnregisterAndDisableHandler(this);
        }
    }
}
