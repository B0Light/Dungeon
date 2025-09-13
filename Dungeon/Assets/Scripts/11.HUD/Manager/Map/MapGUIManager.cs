using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapGUIManager : GUIComponent, IDragHandler, IScrollHandler
{
    [Header("Map Settings")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 5f;

    private float _currentZoom = 1f;
    private RectTransform _parentRect;

    private void Start()
    {
        if (mapContent != null)
        {
            _parentRect = (RectTransform)mapContent.parent;
            _currentZoom = mapContent.localScale.x; // 초기 스케일 값으로 설정
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (mapContent == null) return;

        // 마우스 위치를 부모 RectTransform 기준 로컬 좌표로 변환
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentRect, 
            eventData.position, 
            eventData.pressEventCamera, 
            out mousePos
        );

        // 줌 변경 전 마우스 위치 (맵 콘텐츠 기준)
        Vector2 mouseLocalPos = (mousePos - mapContent.anchoredPosition) / _currentZoom;

        // 스크롤 값으로 줌 계산
        float scrollValue = eventData.scrollDelta.y;
        float zoomFactor = 1f + (zoomSpeed * scrollValue);
        
        float newZoom = _currentZoom * zoomFactor;
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        
        // 실제 줌 변경량 계산
        float actualZoomFactor = newZoom / _currentZoom;
        _currentZoom = newZoom;

        // 스케일 변경 (Stretch 모드에서도 동작)
        mapContent.localScale = Vector3.one * _currentZoom;

        // 마우스 위치를 기준으로 위치 조정
        Vector2 newMouseLocalPos = mouseLocalPos * _currentZoom;
        Vector2 deltaPos = (mousePos - mapContent.anchoredPosition) - newMouseLocalPos;
        mapContent.anchoredPosition += deltaPos;

        ClampPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mapContent == null) return;
        
        // 스케일에 따른 드래그 감도 조정
        Vector2 scaledDelta = eventData.delta / _currentZoom;
        mapContent.anchoredPosition += scaledDelta;
        ClampPosition();
    }

    private void ClampPosition()
    {
        if (mapContent == null || _parentRect == null) return;

        // 부모 캔버스 크기
        Vector2 parentSize = _parentRect.rect.size;
        
        // 현재 스케일을 적용한 실제 콘텐츠 크기
        Vector2 scaledContentSize = mapContent.rect.size * _currentZoom;
        
        // 콘텐츠가 부모보다 작을 때는 중앙에 고정
        Vector2 currentPos = mapContent.anchoredPosition;
        
        if (scaledContentSize.x <= parentSize.x)
        {
            currentPos.x = 0;
        }
        else
        {
            float maxOffsetX = (scaledContentSize.x - parentSize.x) / 2f;
            currentPos.x = Mathf.Clamp(currentPos.x, -maxOffsetX, maxOffsetX);
        }

        if (scaledContentSize.y <= parentSize.y)
        {
            currentPos.y = 0;
        }
        else
        {
            float maxOffsetY = (scaledContentSize.y - parentSize.y) / 2f;
            currentPos.y = Mathf.Clamp(currentPos.y, -maxOffsetY, maxOffsetY);
        }

        mapContent.anchoredPosition = currentPos;
    }

    // 줌을 초기값으로 리셋하는 메서드
    public void ResetZoom()
    {
        if (mapContent == null) return;
        
        _currentZoom = 1f;
        mapContent.localScale = Vector3.one;
        mapContent.anchoredPosition = Vector2.zero;
    }

    // 현재 줌 레벨을 반환하는 메서드
    public float GetCurrentZoom()
    {
        return _currentZoom;
    }

    // 특정 줌 레벨로 설정하는 메서드 (선택사항)
    public void SetZoom(float targetZoom)
    {
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        _currentZoom = targetZoom;
        mapContent.localScale = Vector3.one * _currentZoom;
        ClampPosition();
    }
}