using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class MenuController: MonoBehaviour
{
    [Header("References")]
    private readonly List<Selectable> _selectedObjectHistory = new List<Selectable>();
    private readonly int _maxHistorySize = 1000;
    private Selectable _lastSelectedObject;
    private Selectable _currentSelectedObject;
    private Selectable GetLastSelectedObject()
    {
        for (int i = _selectedObjectHistory.Count-1; i >= 0; --i)
        {
            if(_selectedObjectHistory[i] != null && _selectedObjectHistory[i].gameObject.activeInHierarchy)
            {
                return _selectedObjectHistory[i];
            }

            _selectedObjectHistory.RemoveAt(i);
        }

        return null;
    }

    private void Start()
    {
        _currentSelectedObject = EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;
        _lastSelectedObject = null;
        _currentSelectedObject?.Select();
    }

    private void Update()
    {
        _lastSelectedObject = GetLastSelectedObject();
        _currentSelectedObject = EventSystem.current.currentSelectedGameObject != null ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;

        if (_currentSelectedObject != null &&
            _currentSelectedObject.gameObject.activeInHierarchy &&
            _lastSelectedObject != _currentSelectedObject)
        {
            if (_selectedObjectHistory.Contains(_currentSelectedObject))
            {
                _selectedObjectHistory.Remove(_currentSelectedObject);
            }
            _selectedObjectHistory.Add(_currentSelectedObject);

            if (_selectedObjectHistory.Count > _maxHistorySize)
            {
                _selectedObjectHistory.RemoveAt(0);
            }
        }
        else if (_lastSelectedObject != null)
        {
            // Using the new Input System to detect key presses
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
                {
                    _lastSelectedObject.Select();
                    return;
                }
                if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    _lastSelectedObject.Select();
                    return;
                }
                if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    _lastSelectedObject.Select();
                    return;
                }
                if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    _lastSelectedObject.Select();
                    return;
                }
            }
        }
    }
}