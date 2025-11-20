using UnityEngine;

public class PlayerUIStatusManager : MonoBehaviour
{
    [Header("STAT BARS")]
    [SerializeField] private UI_StatBarSlider healthBar;
    [SerializeField] private UI_StatBarSlider staminaBar;

    public void SetNewHealthValue(float newValue)
    {
        healthBar.SetStat(newValue);
    }

    public void SetMaxHealthValue(float maxHealth)
    {
        healthBar.SetMaxStat(maxHealth);
    }

    public void SetNewStamina(float newValue)
    {
        staminaBar.SetStat(newValue);
    }

    public void SetMaxStamina(float newValue)
    {
        staminaBar.SetMaxStat(newValue);
    }
}
