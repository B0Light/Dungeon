using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private MovementSystem movementSystem;

    public bool PlayersTurn { get; private set; } = true;
    
    private Unit _unitShip;
    private Hex _previouslySelectedHex;

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
        if (_unitShip == unitReference)
        {
            ClearOldSelection();
            return true;
        }
        return false;
    }

    public void HandleTerrainSelected(GameObject hexGameObject)
    {
        if (_unitShip == null || PlayersTurn == false)
        {
            return;
        }

        Hex selectedHex = hexGameObject.GetComponent<Hex>();

        if (HandleHexOutOfRange(selectedHex.HexCoords) || HandleSelectedHexIsUnitHex(selectedHex.HexCoords))
            return;

        HandleTargetHexSelected(selectedHex);

    }

    private void PrepareUnitForMovement(Unit unitReference)
    {
        if (_unitShip != null)
        {
            ClearOldSelection();
        }

        _unitShip = unitReference;
        _unitShip.Select();
        movementSystem.ShowRange(_unitShip, hexGrid);
    }

    private void ClearOldSelection()
    {
        _previouslySelectedHex = null;
        _unitShip.Deselect();
        movementSystem.HideRange(hexGrid);
        _unitShip = null;

    }

    private void HandleTargetHexSelected(Hex selectedHex)
    {
        Debug.Log("HandelTargetHexSelected");
        if (_previouslySelectedHex == null || _previouslySelectedHex != selectedHex)
        {
            _previouslySelectedHex = selectedHex;
            movementSystem.ShowPath(selectedHex.HexCoords, hexGrid);
        }
        else
        {
            movementSystem.MoveUnit(_unitShip, hexGrid);
            PlayersTurn = false;
            _unitShip.MovementFinished += ResetTurn;
            ClearOldSelection();
        }
    }

    private bool HandleSelectedHexIsUnitHex(HexCoordinate hexPosition)
    {
        if (hexPosition == hexGrid.GetClosestHex(_unitShip.transform.position))
        {
            _unitShip.Deselect();
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
