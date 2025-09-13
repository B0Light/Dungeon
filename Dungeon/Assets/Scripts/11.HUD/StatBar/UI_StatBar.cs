using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class UI_StatBar : MonoBehaviour
{
    [SerializeField] private Color red;
    [SerializeField] private Color green;
    [SerializeField] private Color blue;

    public abstract void SetStat(int newValue);

    public abstract void SetMaxStat(int maxValue);
    
    
    protected Color GetColorGradient(int value)
    {
        float normalizedValue = Mathf.Clamp01(value / 1000f);

        if (normalizedValue <= 0.1f)
        {
            return red;
        }
        else if (normalizedValue <= 0.5f)
        {
            return Color.Lerp(red, green, (normalizedValue - 0.1f) * 2f);
        }
        else
        {
            return Color.Lerp(green, blue, (normalizedValue - 0.5f) * 2f);
        }
    }
}