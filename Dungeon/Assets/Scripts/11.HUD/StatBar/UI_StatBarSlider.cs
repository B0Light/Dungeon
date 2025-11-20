using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_StatBarSlider : MonoBehaviour
{
    private Slider _slider;
    private Coroutine _lerpRoutine;

    protected virtual void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
    }

    public void SetStat(float newValue)
    {
        if (_lerpRoutine != null)
        {
            StopCoroutine(_lerpRoutine);
        }
        
        _lerpRoutine = StartCoroutine(UpdateBarRoutine(newValue));
    }

    public void SetMaxStat(float maxValue)
    {
        _slider.maxValue = maxValue;
        SetStat(maxValue);
    }

    private IEnumerator UpdateBarRoutine(float targetValue)
    {
        float startValue = _slider.value;
        float time = 0f;
        float duration = 0.5f; // 0.5초 동안 이동

        while (time < duration)
        {
            time += Time.deltaTime;
            _slider.value = Mathf.Lerp(startValue, targetValue, time / duration);
            yield return null;
        }

        _slider.value = targetValue;
        _lerpRoutine = null;
    }
}
