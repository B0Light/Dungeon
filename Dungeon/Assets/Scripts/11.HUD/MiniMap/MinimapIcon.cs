using System;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    private Transform _minimapCamera;
    
    private void Start()
    {
        SetMiniMapCamera();
    }

    private void SetMiniMapCamera()
    {
        if(PlayerCameraController.Instance == null) return;
        _minimapCamera = PlayerCameraController.Instance.transform;
    }
    
    // summary 에서 호출 
    private void SetMiniMapCamera(Transform camTransform)
    {
        if(camTransform == null) return;
        _minimapCamera = camTransform;
    }

    void LateUpdate()
    {
        // 미니맵 카메라의 y축 회전만 따라가고 나머지는 고정
        Vector3 rotation = _minimapCamera.eulerAngles;
        transform.rotation = Quaternion.Euler(90f, rotation.y, 0f);
    }
}

