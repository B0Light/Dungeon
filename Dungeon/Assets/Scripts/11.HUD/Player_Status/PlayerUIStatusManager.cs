using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerUIStatusManager : MonoBehaviour
{
    [Header("STAT BARS")]
    [SerializeField] private UI_StatBarSlider healthBar;
    [SerializeField] private UI_StatBarSlider staminaBar;

    public void SetNewHealthValue(int newValue)
    {
        healthBar.SetStat(newValue);
    }

    public void SetMaxHealthValue(int maxHealth)
    {
        healthBar.SetMaxStat(maxHealth);
    }

    public void SetNewActionPoint(int newValue)
    {
        staminaBar.SetStat(newValue);
    }

    public void SetMaxActionPoint(int newValue)
    {
        staminaBar.SetMaxStat(newValue);
    }
}
