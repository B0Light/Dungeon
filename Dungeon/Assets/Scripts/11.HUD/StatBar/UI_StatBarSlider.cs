using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_StatBarSlider : MonoBehaviour
{
    private Slider _slider;
    private Coroutine _lerpRoutine;
    [SerializeField] private float _changeSpeed = 5f; // 값 변화 속도 조절

    protected virtual void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
    }

    public void SetStat(float newValue)
    {
        StopAllCoroutines();
        _lerpRoutine = StartCoroutine(LerpValue(newValue));
    }

    public void SetMaxStat(float maxValue)
    {
        _slider.maxValue = maxValue;
        SetStat(maxValue); // 초기 설정도 애니메이션 가능
    }

    private IEnumerator LerpValue(float targetValue)
    {
        float startValue = _slider.value;

        while (Mathf.Abs(_slider.value - targetValue) > 0.01f)
        {
            _slider.value = Mathf.Lerp(_slider.value, targetValue, Time.deltaTime * _changeSpeed);
            yield return null;
        }

        _slider.value = targetValue;
        _lerpRoutine = null;
    }
}
