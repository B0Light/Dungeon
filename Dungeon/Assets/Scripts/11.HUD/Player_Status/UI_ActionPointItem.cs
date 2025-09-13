using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ActionPointItem : MonoBehaviour
{
    [SerializeField] private Image actionPointObject;
    [SerializeField] private Color activateColor;
    [SerializeField] private Color deactivateColor;
    private void OnEnable()
    {
        actionPointObject.color = activateColor;
    }

    public void UseActionPoint()
    {
        actionPointObject.color = deactivateColor;
    }

    public void RegainActionPoint()
    {
        actionPointObject.color = activateColor;
    }
}
