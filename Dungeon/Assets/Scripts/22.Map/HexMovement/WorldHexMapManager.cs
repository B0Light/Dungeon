using System;
using UnityEngine;

public class WorldHexMapManager : Singleton<WorldHexMapManager>
{
    public Camera hexMapCamera;

    [SerializeField] private GameObject unitObject;
    
    public HexCoordinate curUnitPos = new HexCoordinate(0,0);

    private void OnEnable()
    {
        unitObject.transform.position = curUnitPos.ConvertToVector3();
    }

    private void EnterTile()
    {
        curUnitPos = HexCoordinate.ConvertFromVector3(unitObject.transform.position);
    }
}
