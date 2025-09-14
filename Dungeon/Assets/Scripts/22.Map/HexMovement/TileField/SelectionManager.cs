using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    private Camera hexCamera;

    public LayerMask selectionMask;
 
    public UnityEvent<GameObject> OnUnitSelected;
    public UnityEvent<GameObject> TerrainSelected;

    private void Awake()
    {
        if (hexCamera == null)
            hexCamera = WorldHexMapManager.Instance.hexMapCamera;
    }

    public void HandleClick(Vector3 mousePosition)
    {
        if (FindTarget(mousePosition, out GameObject result))
        {
            if (UnitSelected(result))
            {
                OnUnitSelected?.Invoke(result);
            }
            else
            {
                TerrainSelected?.Invoke(result);
            }
        }
    }

    private bool UnitSelected(GameObject result)
    {
        return result.GetComponent<Unit>() != null;
    }

    private bool FindTarget(Vector3 mousePosition, out GameObject result)
    {
        Ray ray = hexCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, selectionMask))
        {
            result = hit.collider.gameObject;
            return true;
        }
        result = null;
        return false;
    }
}
