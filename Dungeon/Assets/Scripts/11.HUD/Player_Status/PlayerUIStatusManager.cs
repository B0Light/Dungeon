using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIStatusManager : MonoBehaviour
{
    [Header("STAT BARS")]
    [SerializeField] private UI_StatBar healthBar;
    [SerializeField] private UI_ActionPointController actionPointBar;
    [SerializeField] private Image hungryLevelIcon;
    [SerializeField] private Image hungryLevel;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color warningBackgroundColor;
    private void Start()
    {
        hungryLevel.fillAmount = 1;
    }

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
        actionPointBar.SetStat(newValue);
    }

    public void SetMaxActionPoint(int newValue)
    {
        actionPointBar.SetMaxStat(newValue);
    }

    public void SetHungryLevel(int value)
    {
        float hungryLevelValue = value / 100f;
        hungryLevel.fillAmount = hungryLevelValue;
    }
    
    public void SetWarningHungryLevel(bool warning)
    {
        hungryLevelIcon.color = warning ? warningBackgroundColor : defaultColor;
    }
}
