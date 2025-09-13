using System;
using UnityEngine;
using UnityEngine.Events;

public class WorldTimeManager : Singleton<WorldTimeManager>
{
    [SerializeField] private Material daySkybox;  // 낮 스카이박스
    [SerializeField] private Material sunsetSkybox;
    [SerializeField] private Material nightSkybox; // 밤 스카이박스

    public Variable<int> day = new Variable<int>(0);
    private int _elapsedDate = 0; 
    // 시간이 바뀔때 마다 1씩 추가
    // 저장 값 : 4 -> 2일차 일몰
    
    private TimeOfDay _currentTime = TimeOfDay.Night;
    public delegate void TimeChangeHandler(TimeOfDay newTime);
    public event TimeChangeHandler OnTimeChanged;
    

    protected override void Awake()
    {
        base.Awake();
        _currentTime = TimeOfDay.Night;
    }

    private void OnEnable()
    {
        OnTimeChanged += OnTimeChange;
    }

    private void OnDisable()
    {
        OnTimeChanged = null;
    }

    [ContextMenu("TimeFlow")]
    public void AdvanceTime()
    {
        switch (_currentTime)
        {
            case TimeOfDay.Day:
                _currentTime = TimeOfDay.Sunset;
                break;
            case TimeOfDay.Sunset:
                _currentTime = TimeOfDay.Night;
                break;
            case TimeOfDay.Night:
                day.Value += 1;
                _currentTime = TimeOfDay.Day;
                break;
        }
        _elapsedDate += 1;
        OnTimeChanged?.Invoke(_currentTime);
    }

    public void SetTime(TimeOfDay timeOfDay)
    {
        if (_currentTime == timeOfDay) return;
        if (timeOfDay < _currentTime) day.Value += 1;
        _elapsedDate += (3 + timeOfDay - _currentTime) % 3;
        
        _currentTime = timeOfDay;
        OnTimeChanged?.Invoke(_currentTime);
    }

    private void OnTimeChange(TimeOfDay curTime)
    {
        _currentTime = curTime;
        ApplySkybox();
    }
    
    public void ApplySkybox()
    {
        Material newSkyBox = daySkybox;
        switch (_currentTime)
        {
            case TimeOfDay.Day:
                newSkyBox = daySkybox;
                break;
            case TimeOfDay.Sunset:
                newSkyBox = sunsetSkybox;
                break;
            case TimeOfDay.Night:
                newSkyBox = nightSkybox;
                break;
        }
        ChangeSkybox(newSkyBox);
    }
    
    private void ChangeSkybox(Material newSkybox)
    {
        RenderSettings.skybox = newSkybox;
        DynamicGI.UpdateEnvironment();  // 글로벌 일루미네이션 업데이트
    }
    
    public string GetTimeAsString()
    {
        return _currentTime.ToString();
    }

    public void LoadGameDate(int date)
    {
        _elapsedDate = date;
    }

    public int GetPlayedDate()
    {
        return _elapsedDate;
    }
}