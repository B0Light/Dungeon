using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Hex : MonoBehaviour
{
    [SerializeField]
    private GlowHighlight highlight;

    [SerializeField]
    private HexType hexType;

    public string hexMapName;
    
    public HexCoordinate HexCoords {
        get {
            return HexCoordinate.ConvertFromVector3(transform.position);
        }
    }

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
        highlight = GetComponent<GlowHighlight>();
        
        HexGrid.Instance.AddTile(this);
    }
    public void EnableHighlight()
    {
        highlight.ToggleGlow(true);
    }

    public void DisableHighlight()
    {
        highlight.ToggleGlow(false);
    }

    internal void ResetHighlight()
    {
        highlight.ResetGlowHighlight();
    }

    internal void HighlightPath()
    {
        highlight.HighlightValidPath();
    }
    
    public void OnMouseToggle()
    {
        if (IsObstacle()) return;
        if(highlight)
            highlight.OnMouseToggleGlow();
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