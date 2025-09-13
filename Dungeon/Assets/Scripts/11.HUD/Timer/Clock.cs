using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private float timeToRefreshInSeconds = 1;
    private bool _beat;
    
    private void OnEnable()
    {
        StartCoroutine(C_UpdateTime());
    }

    private void OnDisable()
    {
        StopCoroutine(C_UpdateTime());
    }
    
    private IEnumerator C_UpdateTime()
    {
        while (true)
        {
            UpdateTime();
            _beat = !_beat;
            yield return new WaitForSecondsRealtime(timeToRefreshInSeconds);
        }
    }
    
    private void UpdateTime()
    {
        // 이 버전에서는 아침 / 낮 / 밤으로 표시됨 
        clockText.SetText(WorldTimeManager.Instance.GetTimeAsString());
    }
}
