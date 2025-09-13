using System;
using UnityEngine;

public class WorldHexMapManager : Singleton<WorldHexMapManager>
{
    public Camera hexMapCamera;

    [SerializeField] private GameObject unitObject;
    
    public HexCoordinate CurUnitPos = new HexCoordinate(0,0);

    private void OnEnable()
    {
        unitObject.transform.position = CurUnitPos.ConvertToVector3();
    }

    private void EnterTile()
    {
        CurUnitPos = HexCoordinate.ConvertFromVector3(unitObject.transform.position);
    }
}
