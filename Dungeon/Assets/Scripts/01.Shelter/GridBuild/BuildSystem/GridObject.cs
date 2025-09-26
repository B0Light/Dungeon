using UnityEngine;

public class GridObject : IPathNode
{
    private GridXZ<GridObject> _grid;
    private int _posX, _posZ; // 현재 GridObject의 위치
    private BuildObjData.Dir _dir;
    private PlacedObject _placedObject;
    private BuildObjData _buildObjData;
    
    /* A* Pathfinding */
    public float GCost { get; set; } // 이동 비용
    public float HCost { get; set; } // 휴리스틱 비용
    public float FCost => GCost + HCost; // 총 비용
    public GridObject Parent { get; set; }
    IPathNode IPathNode.Parent { get => Parent; set => Parent = (GridObject)value; }
    
    public GridObject(GridXZ<GridObject> grid, int posX, int posZ)
    {
        _grid = grid;
        _posX = posX;
        _posZ = posZ;
        _dir = BuildObjData.Dir.Down;
        _placedObject = null;
    }

    public override string ToString() {
        return _posX + ", " + _posZ + "\n" + _placedObject;
    }

    public void SetPlacedObject(PlacedObject placedObject, BuildObjData buildObjData, BuildObjData.Dir dir)
    {
        _placedObject = placedObject;
        _buildObjData = buildObjData;
        _dir = dir;
    }

    public TileType? GetTileType()
    {
        return _buildObjData?.GetTileType();
    }

    public PlacedObject GetPlacedObject()
    {
        return _placedObject;
    }

    public void ClearPlacedObject()
    {
        _placedObject = null;
        _buildObjData = null;
        _dir = BuildObjData.Dir.Down;
    }

    // 현재 타일에 건설된 건물이 없거나 default(제거되거나)
    public bool CanBuild()
    { 
        return _placedObject == null || _placedObject.IsDefault();
    }

    public Vector2Int GetEntrancePosition() => _placedObject != null ?
        _placedObject.GetEntrance() : new Vector2Int(_posX, _posZ);
    
    public Vector2Int GetExitPosition() => _placedObject != null ?
        _placedObject.GetExit() : new Vector2Int(_posX, _posZ);

    public BuildObjData.Dir GetDirection() => _dir;

    public BuildObjData.Dir GetExitDirection() => _placedObject.GetActualExitDirection();

}