using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseLoadingScene : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            OnAnyKeyPressed();  
        }
    }

    private void OnAnyKeyPressed()
    {
        WorldSceneChangeManager.Instance.CloseLoadingScreen();
        PlayerInputManager.Instance.EnableCharacter();
    }
}
