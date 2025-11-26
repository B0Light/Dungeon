using System.Collections.Generic;
using UnityEngine;

public class DungeonGridBuildSystem : BaseGridBuildSystem
{
    [SerializeField] private DungeonManager dungeonManager;

    protected override void Awake()
    {
        base.Awake();
        dungeonManager.OnGeneratedDungeon += OnGeneratedDungeon;
    }

    private void OnGeneratedDungeon()
    {
        Debug.Log("[Dungeon Grid Build System] : Set Fixed Grid");
        _fixedGrid = dungeonManager.FixedGrid;
    }
    
    public override bool CanBuildAtPos(List<Vector2Int> gridPositionList)
    {
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
            if (gridObject == null || 
                !gridObject.CanBuild() || 
                gridObject.CellType != CellType.Floor)
            {
                return false;
            }
        }
        return true;
    }
}
