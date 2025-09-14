using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    private GlowHighlight _highlight;

    [SerializeField]
    private HexType hexType;

    public string hexMapName;
    
    public HexCoordinate HexCoords => HexCoordinate.ConvertFromVector3(transform.position);

    public int GetCost()
        => hexType switch
        {
            HexType.Difficult => 20,
            HexType.Default => 10,
            HexType.Road => 5,
            HexType.Water01 => 5,
            HexType.Water02 => 10,
            HexType.Water03 => 15,
            HexType.Dock => 5,
            _ => throw new Exception($"Hex of type {hexType} not supported")
        };

    public bool IsObstacle()
    {
        return this.hexType == HexType.Obstacle;
    }

    public bool IsDock()
    {
        return hexType == HexType.Dock;
    }

    private void Awake()
    {
        _highlight = GetComponent<GlowHighlight>();
        
        HexGrid.Instance.AddTile(this);
    }
    public void EnableHighlight()
    {
        _highlight.ToggleGlow(true);
    }

    public void DisableHighlight()
    {
        _highlight.ToggleGlow(false);
    }

    internal void ResetHighlight()
    {
        _highlight.ResetGlowHighlight();
    }

    internal void HighlightPath()
    {
        _highlight.HighlightValidPath();
    }
    
    public void OnMouseToggle()
    {
        if (IsObstacle()) return;
        if(_highlight)
            _highlight.OnMouseToggleGlow();
    }
}

public enum HexType
{
    None,
    Default,
    Difficult,
    Road,
    Water01,
    Water02,
    Water03,
    Obstacle,
    Dock,
}