using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : Singleton<UIManager>
{
    [Header("MouseCursor")]
    [SerializeField] private Texture2D customCursor; // 사용할 커서 이미지
    private readonly Vector2 _hotspot = Vector2.zero; // 커서 중심 위치
    private HUDComponent _currentActiveHUD;
    
    private void Start()
    {
        if (customCursor != null)
        {
            Cursor.SetCursor(customCursor, _hotspot, CursorMode.Auto);
        }
    }

    public void MouseActive(bool active)
    {
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;    
    }
}
