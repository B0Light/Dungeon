
public class PlayerStatsManager : CharacterStatsManager
{
    public void SetNewHealthPoint(float value)
    {
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetNewHealthValue(value);
    }

    public void SetNewStamina(float value)
    {
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetNewStamina(value);
    }
}

