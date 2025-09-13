using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_StatBarSlider : UI_StatBar
{
    private Image icon;
    protected Slider slider;
    
    protected virtual void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        icon = GetComponentInChildren<Image>();
    }
    

    public override void SetStat(int newValue)
    {
        slider.value = newValue;
        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = GetColorGradient(newValue);
        }
    }

    public override void SetMaxStat(int maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;
        Image fillImage = slider.fillRect.GetComponent<Image>();
        
        if (fillImage != null)
        {
            fillImage.color = GetColorGradient(maxValue);
        }

        if (icon != null)
        {
            icon.color = GetColorGradient(maxValue);
        }
    }
}