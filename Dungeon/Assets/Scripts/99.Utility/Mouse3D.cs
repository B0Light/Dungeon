using System;
using UnityEngine;

public class Mouse3D : MonoBehaviour
{
    public static Mouse3D Instance { get; private set; }
    
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Vector3 defaultPosition = new Vector3(-100, -100, -100);

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateMousePosition();
    }

    private void UpdateMousePosition()
    {
        Ray ray = Camera.main?.ScreenPointToRay(Input.mousePosition) ?? default;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            transform.position = raycastHit.point;
        }
    }

    public static Vector3 GetMouseWorldPosition()
    {
        if (Instance == null)
        {
            Debug.LogError("Mouse3D Instance is null. Ensure Mouse3D is attached to an active GameObject.");
            return Vector3.zero;
        }
        return Instance.GetMouseWorldPosition_Instance();
    }

    private Vector3 GetMouseWorldPosition_Instance()
    {
        Ray ray = Camera.main?.ScreenPointToRay(Input.mousePosition) ?? default;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, layerMask))
        {
            return raycastHit.point;
        }
        else
        {
            return defaultPosition;
        }
    }
}