using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuadrantZoom : MonoBehaviour
{
    public RectTransform content;  // 전체 캔버스 (부모)
    public RectTransform[] quadrants; // 4개의 사분면
    private Vector2 _originalPos;  // 원래 위치 저장
    private Vector3 _originalScale; // 원래 크기 저장
    private bool _isZoomed = false; // 확대 상태 체크
    private readonly float _zoomDuration = 0.75f; // 확대 애니메이션 속도

    private Coroutine zoomCoroutine;

    private void Start()
    {
        _originalPos = content.anchoredPosition;
        _originalScale = content.localScale;
    }

    [ContextMenu("ZoomIN")]
    public void Zoom1()
    {
        ZoomToQuadrant(0);
    }

    public void ZoomToQuadrant(int quadrantIndex)
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        if (_isZoomed) // 이미 확대된 경우 -> 원래 크기로 복귀
        {
            zoomCoroutine = StartCoroutine(SmoothZoom(_originalScale, _originalPos, new Vector2(0.5f, 0.5f)));
        }
        else // 클릭한 사분면을 확대
        {
            RectTransform selectedQuadrant = quadrants[quadrantIndex];

            // 해당 사분면을 기준으로 Pivot 변경
            Vector2 targetPivot = GetPivotForQuadrant(quadrantIndex);
            content.pivot = targetPivot;

            // 사분면의 위치를 기준으로 확대될 새로운 중심 위치 계산
            Vector2 targetPos = -selectedQuadrant.anchoredPosition * 2; 

            zoomCoroutine = StartCoroutine(SmoothZoom(new Vector3(2f, 2f, 1f), targetPos, targetPivot));
        }

        _isZoomed = !_isZoomed;
    }

    private Vector2 GetPivotForQuadrant(int index)
    {
        switch (index)
        {
            case 0: return new Vector2(0, 1); // 1사분면 (좌상단)
            case 1: return new Vector2(1, 1); // 2사분면 (우상단)
            case 2: return new Vector2(0, 0); // 3사분면 (좌하단)
            case 3: return new Vector2(1, 0); // 4사분면 (우하단)
            default: return new Vector2(0.5f, 0.5f);
        }
    }

    private IEnumerator SmoothZoom(Vector3 targetScale, Vector2 targetPos, Vector2 targetPivot)
    {
        float time = 0f;
        Vector3 startScale = content.localScale;
        Vector2 startPos = content.anchoredPosition;

        while (time < _zoomDuration)
        {
            time += Time.deltaTime;
            float t = time / _zoomDuration;
            content.localScale = Vector3.Lerp(startScale, targetScale, t);
            content.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        content.localScale = targetScale;
        content.anchoredPosition = targetPos;
    }
}
