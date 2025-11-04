using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

    public void SetNewActionPoint(float newValue)
    {
        staminaBar.SetStat(newValue);
    }

    public void SetMaxActionPoint(float newValue)
    {
        staminaBar.SetMaxStat(newValue);
    }
}
