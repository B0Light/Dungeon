using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput_Hex : MonoBehaviour
{
    public UnityEvent<Vector3> PointerClick;
    
    public LayerMask selectionMask;
    private Hex hex = null;
    GameObject originObj = null;

    void Update()
    {
        MouseMove();
        DetectMouseClick();
    }

    private void DetectMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            PointerClick?.Invoke(mousePos);
        }
    }
    
    private void MouseMove()
    {
        Ray ray = WorldHexMapManager.Instance.hexMapCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100, selectionMask)) return;

        GameObject selectedObject = hit.collider.gameObject;

        if (originObj == selectedObject) return;

        if (hex != null)
        {
            // origin off
            hex.OnMouseToggle();
        }

        hex = selectedObject.GetComponent<Hex>();

        if (hex != null)
        {
            // New Obj on
            hex.OnMouseToggle();
        }

    }
}
