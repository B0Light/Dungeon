using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUIManager : MonoBehaviour
{
    [Header("UI")]
    public bool hasFloatingHPBar = false;
    public UI_CharacterHPBar characterHPBar;

    public void OnHPChanged(float newValue)
    {
        characterHPBar.SetStat(newValue);
    }
}
