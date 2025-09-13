using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogWallInteractable : MonoBehaviour
{
    [Header("FOG")]
    [SerializeField] private GameObject[] fogGameObjects;

    [Header("ID")] public int fogWallID;
    
    [Header("Active")]
    public Variable<bool> isActive = new Variable<bool>(false);

    private void OnEnable()
    {
        OnIsActiveChanged(isActive.Value);
        isActive.OnValueChanged += OnIsActiveChanged;
        WorldObjectManager.Instance.AddFogWallToList(this);
    }

    private void OnDisable()
    {
        isActive.OnValueChanged -= OnIsActiveChanged;
        WorldObjectManager.Instance.RemoveFogWallToList(this);
    }
    
    private void OnIsActiveChanged(bool newStatus)
    {
        Debug.Log("FOG WALL VALUE CHANGED");
        if (isActive.Value)
        {
            foreach (var fogObject in fogGameObjects)
            {
                fogObject.SetActive(true);
            }
        }
        else
        {
            foreach (var fogObject in fogGameObjects)
            {
                fogObject.SetActive(false);
            }
        }
    }
}
