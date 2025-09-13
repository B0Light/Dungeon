using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI_StatBarImage : UI_StatBar
{
    private const float MaxValueCoef = 0.87f;
    private int _maxValue = 100;
    
    [SerializeField] private Image icon;
    [SerializeField] private Image slider;
    [SerializeField] private TextMeshProUGUI valueText;

    public override void SetStat(int newValue)
    {
        float value = (float)newValue / _maxValue;
        slider.fillAmount = value * MaxValueCoef;
        valueText.text = (int)(value * 100f) + "%";
        slider.color = GetColorGradient(_maxValue);
        icon.color = GetColorGradient(_maxValue);
    }

    public override void SetMaxStat(int maxValue)
    {
        _maxValue = maxValue;
        slider.fillAmount = MaxValueCoef;
        valueText.text = "100%";
        slider.color = GetColorGradient(maxValue);
        icon.color = GetColorGradient(maxValue);
    }
}
