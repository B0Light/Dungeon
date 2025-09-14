using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput_Hex : MonoBehaviour
{
    public LayerMask selectionMask;
    private Hex hex = null;
    GameObject originObj = null;
    
    public UnityEvent<Vector3> pointerClick;

    void Awake()
    {
        // pointerClick이 Inspector에서 할당되지 않았을 경우,
        // 새로운 UnityEvent 인스턴스를 생성하여 NullReferenceException을 방지합니다.
        if (pointerClick == null)
        {
            pointerClick = new UnityEvent<Vector3>();
        }
    }

    void Update()
    {
        MouseMove();
        DetectMouseClick();
    }

    private void DetectMouseClick()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos2D = Mouse.current.position.ReadValue();
            Vector3 mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0f);
            
            // `pointerClick` 변수가 null이 아닌지 확인한 후 호출합니다.
            // Awake()에서 초기화했기 때문에 이 검사는 사실상 항상 true가 됩니다.
            pointerClick?.Invoke(mousePos);
        }
    }
    
    private void MouseMove()
    {
        if (Mouse.current == null) return;
        Vector2 mousePos2D = Mouse.current.position.ReadValue();
        Vector3 mousePos = new Vector3(mousePos2D.x, mousePos2D.y, 0f);
        
        // WorldHexMapManager.Instance가 null일 수도 있으니 추가 확인하는 것이 좋습니다.
        if (WorldHexMapManager.Instance == null || WorldHexMapManager.Instance.hexMapCamera == null)
        {
            Debug.LogError("WorldHexMapManager or its camera is not initialized.");
            return;
        }

        Ray ray = WorldHexMapManager.Instance.hexMapCamera.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100, selectionMask)) return;

        GameObject selectedObject = hit.collider.gameObject;

        if (originObj == selectedObject) return;

        if (hex != null)
        {
            hex.OnMouseToggle();
        }

        hex = selectedObject.GetComponent<Hex>();

        if (hex != null)
        {
            hex.OnMouseToggle();
        }
    }
}