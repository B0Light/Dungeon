using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedTimerUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image hourglassIcon; // 모래시계 아이콘 Image 컴포넌트
    
    [Header("Timer Settings")]
    [SerializeField] private float gameTime = 300f; // 게임 시간 (초)
    [SerializeField] private bool countDown = true;
    
    [Header("Animation Settings")]
    [SerializeField] private float rotationCycleDuration = 1f; // 회전 사이클 전체 시간 (1초)
    [SerializeField] private float rotationDuration = 0.5f; // 실제 회전하는 시간 (0.5초)
    [SerializeField] private bool rotateClockwise = true; // 시계방향 회전
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 회전 애니메이션 커브
    
    [Header("Visual Effects")]
    [SerializeField] private bool pulseEffect = false; // 펄스 효과
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScale = 1.1f;
    
    private float currentTime;
    private bool isTimerRunning = false;
    private Vector3 originalScale;
    private float rotationTimer = 0f; // 회전 타이머
    
    // 이벤트
    public static event Action OnTimerStart;
    public static event Action OnTimerEnd;
    public static event Action<float> OnTimerUpdate;

    private void Awake()
    {
        // 초기 설정
        currentTime = countDown ? gameTime : 0f;
        if (hourglassIcon != null)
        {
            originalScale = hourglassIcon.transform.localScale;
        }
        
        UpdateTimerDisplay();
        
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
            AnimateHourglassIcon();
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        OnTimerStart?.Invoke();
        Debug.Log("애니메이션 타이머 시작!");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("애니메이션 타이머 정지!");
    }

    public void ResetTimer()
    {
        currentTime = countDown ? gameTime : 0f;
        isTimerRunning = false;
        UpdateTimerDisplay();
        
        // 아이콘 회전 리셋
        if (hourglassIcon != null)
        {
            hourglassIcon.transform.rotation = Quaternion.identity;
            hourglassIcon.transform.localScale = originalScale;
        }
        
        // 회전 타이머 리셋
        rotationTimer = 0f;
        
        Debug.Log("애니메이션 타이머 리셋!");
    }

    private void UpdateTimer()
    {
        if (countDown)
        {
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isTimerRunning = false;
                OnTimerEnd?.Invoke();
                Debug.Log("시간 종료!");
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            
            if (currentTime >= gameTime)
            {
                currentTime = gameTime;
                isTimerRunning = false;
                OnTimerEnd?.Invoke();
                Debug.Log("시간 종료!");
            }
        }
        
        UpdateTimerDisplay();
        OnTimerUpdate?.Invoke(currentTime);
    }

    private void AnimateHourglassIcon()
    {
        if (hourglassIcon == null) return;

        // 회전 사이클 타이머 업데이트
        rotationTimer += Time.deltaTime;
        
        // 회전 애니메이션 (0.5초 동안만 회전)
        if (rotationTimer <= rotationDuration)
        {
            // 0~0.5초: 회전
            float rotationProgress = rotationTimer / rotationDuration;
            float curveValue = rotationCurve.Evaluate(rotationProgress);
            
            float targetRotation = rotateClockwise ? 360f : -360f;
            float currentRotationAngle = targetRotation * curveValue;
            
            hourglassIcon.transform.rotation = Quaternion.Euler(0, 0, currentRotationAngle);
        }
        else if (rotationTimer >= rotationCycleDuration)
        {
            // 1초가 지나면 다음 사이클 시작
            rotationTimer = 0f;
            // 제자리로 정확히 리셋
            hourglassIcon.transform.rotation = Quaternion.identity;
        }
        else
        {
            // 0.5~1초: 정지 상태 - 제자리 유지
            hourglassIcon.transform.rotation = Quaternion.identity;
        }
        
        // 펄스 효과
        if (pulseEffect)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f);
            hourglassIcon.transform.localScale = originalScale * pulse;
        }
        
        // 시간이 얼마 남지 않았을 때 빨간색으로 변경
        if (countDown && currentTime <= 30f && hourglassIcon != null)
        {
            float alpha = Mathf.PingPong(Time.time * 3f, 1f);
            Color warningColor = Color.Lerp(Color.white, Color.red, alpha);
            hourglassIcon.color = warningColor;
        }
        else if (hourglassIcon != null)
        {
            hourglassIcon.color = Color.white;
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // 시간이 얼마 남지 않았을 때 텍스트 색상 변경
            if (countDown && currentTime <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    // 회전 사이클 시간 동적 변경
    public void SetRotationCycleDuration(float cycleDuration)
    {
        rotationCycleDuration = cycleDuration;
    }
    
    // 회전 지속 시간 변경 (사이클 내에서 실제 도는 시간)
    public void SetRotationDuration(float duration)
    {
        rotationDuration = Mathf.Min(duration, rotationCycleDuration);
    }
    
    // 회전 방향 변경
    public void SetRotationDirection(bool clockwise)
    {
        rotateClockwise = clockwise;
    }
    
    // 펄스 효과 토글
    public void TogglePulseEffect(bool enable)
    {
        pulseEffect = enable;
        if (!enable && hourglassIcon != null)
        {
            hourglassIcon.transform.localScale = originalScale;
        }
    }

    // 현재 시간 반환
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // 남은 시간 반환
    public float GetRemainingTime()
    {
        return countDown ? currentTime : gameTime - currentTime;
    }

    // 타이머 실행 상태 반환
    public bool IsRunning()
    {
        return isTimerRunning;
    }
}