using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private HexGrid hexGrid;

    [SerializeField]
    private MovementSystem movementSystem;

    public bool PlayersTurn { get; private set; } = true;

    [SerializeField]
    private Unit unitShip;
    private Hex previouslySelectedHex;

    public void HandleUnitSelected(GameObject unit)
    {
        if (PlayersTurn == false)
            return;

        Unit unitReference = unit.GetComponent<Unit>();

        if (CheckIfTheSameUnitSelected(unitReference))
            return;

        PrepareUnitForMovement(unitReference);
    }

    private bool CheckIfTheSameUnitSelected(Unit unitReference)
    {
        if (unitShip == unitReference)
        {
            ClearOldSelection();
            return true;
        }
        return false;
    }

    public void HandleTerrainSelected(GameObject hexGO)
    {
        if (unitShip == null || PlayersTurn == false)
        {
            return;
        }

        Hex selectedHex = hexGO.GetComponent<Hex>();

        if (HandleHexOutOfRange(selectedHex.HexCoords) || HandleSelectedHexIsUnitHex(selectedHex.HexCoords))
            return;

        HandleTargetHexSelected(selectedHex);

    }

    private void PrepareUnitForMovement(Unit unitReference)
    {
        if (unitShip != null)
        {
            ClearOldSelection();
        }

        unitShip = unitReference;
        unitShip.Select();
        movementSystem.ShowRange(unitShip, hexGrid);
    }

    private void ClearOldSelection()
    {
        previouslySelectedHex = null;
        unitShip.Deselect();
        movementSystem.HideRange(hexGrid);
        unitShip = null;

    }

    private void HandleTargetHexSelected(Hex selectedHex)
    {
        if (previouslySelectedHex == null || previouslySelectedHex != selectedHex)
        {
            previouslySelectedHex = selectedHex;
            movementSystem.ShowPath(selectedHex.HexCoords, hexGrid);
        }
        else
        {
            movementSystem.MoveUnit(unitShip, hexGrid);
            PlayersTurn = false;
            unitShip.MovementFinished += ResetTurn;
            ClearOldSelection();

        }
    }

    private bool HandleSelectedHexIsUnitHex(HexCoordinate hexPosition)
    {
        if (hexPosition == hexGrid.GetClosestHex(unitShip.transform.position))
        {
            unitShip.Deselect();
            ClearOldSelection();
            return true;
        }
        return false;
    }

    private bool HandleHexOutOfRange(HexCoordinate hexPosition)
    {
        if (movementSystem.IsHexInRange(hexPosition) == false)
        {
            Debug.Log("Hex Out of range!");
            return true;
        }
        return false;
    }

    // 여기서 해당 칸이 Dock 칸이면 해당 맵으로 이동하는 매서드 추가 
    private void ResetTurn(Unit selectedUnit)
    {
        selectedUnit.MovementFinished -= ResetTurn;
        PlayersTurn = true;

        HexCoordinate curPos = HexCoordinate.ConvertFromVector3(selectedUnit.transform.position);
        Hex curHex = hexGrid.GetTileAt(curPos);
        if (curHex.IsDock())
        {
            WorldSceneChangeManager.Instance.LoadSceneAsync(curHex.hexMapName);
        }
    }
}
