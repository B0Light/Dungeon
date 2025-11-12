using System;
using System.Collections.Generic;
using UnityEngine;

public class GridBuildSystem : MonoBehaviour
{ 
    public static GridBuildSystem Instance { get; private set; }
    private static GridBuildSystem _instance;

    public BuildObjData ObjectToPlace { get; set; }

    protected FixedGridXZ<GridCell> _fixedGrid;
    private BuildObjData.Dir _dir = BuildObjData.Dir.Down;
    protected readonly int _gridWidth = 7;
    protected readonly int _gridHeight = 9;
    protected readonly int _cellSize = 5;

    // CheckPoint List -> For NPC
    public List<Vector2Int> CheckPointList { get; private set; }

    public static event Action<BuildObjData> OnSelectedChanged;
    public static event Action<BuildObjData> OnObjectPlaced;

    private void Awake()
    {
        Instance = this;
        _fixedGrid = new FixedGridXZ<GridCell>(
            _gridWidth,
            _gridHeight,
            _cellSize,
            transform.position,
            (x, z) => new GridCell(x, z, CellType.Empty)
        );
    }
    
    protected virtual void Start()
    {
        CheckPointList = new List<Vector2Int>();
    }

    #region Place & Remove Tile

    public virtual PlacedObject PlaceTile(int x, int z, BuildObjData.Dir dir, int level = 0, bool isIrremovable = false)
    {
        var gridPositionList = ObjectToPlace.GetGridPositionList(new Vector2Int(x, z), dir);

        if(!CanBuildAtPos(gridPositionList)) return null;
        
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
            gridObject.GetPlacedObject()?.DestroySelf();
            gridObject.ClearPlacedObject();
        }
        
        return BuildTile(x, z, dir, level, isIrremovable);
    }
    
    protected virtual PlacedObject BuildTile(int x, int z, BuildObjData.Dir dir, int level = 0, bool isIrremovable = false)
    {
        Vector2Int rotationOffset = ObjectToPlace.GetRotationOffset(dir);
        Vector3 placedObjectWorldPosition = _fixedGrid.GetWorldPosition(x, z) +
                                            new Vector3(rotationOffset.x, 0, rotationOffset.y) * _fixedGrid.CellSize;

        PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(x, z), dir, ObjectToPlace, level, isIrremovable);

        var gridPositionList = ObjectToPlace.GetGridPositionList(new Vector2Int(x, z), dir);
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            SetObjectAtGridPosition(gridPosition, placedObject, dir);
        }

        OnObjectPlaced?.Invoke(ObjectToPlace);
        return placedObject;
    }
    
    private bool CanBuildAtPos(List<Vector2Int> gridPositionList)
    {
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
            if (gridObject == null || !gridObject.CanBuild())
            {
                return false;
            }
        }
        return true;
    }

    public virtual void RemoveTile(PlacedObject placedObject)
    {
        if (placedObject != null && placedObject.Irremovable == false)
        {
           List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

            placedObject.DestroySelf();
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                ClearObjectAtGridPosition(gridPosition);
            }
        }
    }
    
    // Grid 상에 배치 
    protected void SetObjectAtGridPosition(Vector2Int position, PlacedObject placedObject, BuildObjData.Dir dir)
    {
        var gridObject = _fixedGrid.GetGridObject(position.x, position.y);
        gridObject?.SetPlacedObject(placedObject, ObjectToPlace, dir); // BuildObjData 저장
    }

    // Grid 상에 제거
    protected void ClearObjectAtGridPosition(Vector2Int gridPosition)
    {
        var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
        if (gridObject != null)
        {
            gridObject.ClearPlacedObject();
        }
    }

    #endregion
    
    #region UpgradeTile

    public virtual bool TryUpgrade(PlacedObject placedObject)
    {
        return true;
    }

    #endregion
    
    public void SelectToBuild(BuildObjData buildData)
    {
        ObjectToPlace = buildData;
        OnSelectedChanged?.Invoke(ObjectToPlace);
        
        Debug.Log($"Select : {buildData?.name}");
    }
    
    public Quaternion GetPlacedObjectRotation() {
        return ObjectToPlace ? Quaternion.Euler(0, ObjectToPlace.GetRotationAngle(_dir), 0) : Quaternion.identity;
    }
    
    public FixedGridXZ<GridCell> GetGrid() => _fixedGrid;
    
    public virtual Vector2Int GetEntrancePos() => Vector2Int.zero;
    
    public virtual Vector2Int GetDungeonPos() => Vector2Int.zero;
}