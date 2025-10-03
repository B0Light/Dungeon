using UnityEngine;


public class GridCell : IPathNode
{
    // A* Pathfinding을 위한 속성들
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public GridCell Parent { get; set; }
    IPathNode IPathNode.Parent { get => Parent; set => Parent = (GridCell)value; }
    
    // 그리드와 위치 정보
    private readonly int _posX, _posZ;
    public Vector2Int Position { get; }

    // 타일 및 건물 정보
    public CellType CellType { get; }
    private PlacedObject _placedObject;
    private BuildObjData _buildObjData;
    private BuildObjData.Dir _dir;
    
    // 길찾기 기능
    public bool IsWalkable { get; private set; }

    public GridCell(int posX, int posZ, CellType cellType)
    {
        _posX = posX;
        _posZ = posZ;
        Position = new Vector2Int(posX, posZ);
        CellType = cellType;
        
        GCost = float.MaxValue;
        
        // 초기 이동 가능 여부 설정
        UpdateWalkability();
    }

    // 타일의 이동 가능 여부를 업데이트하는 내부 메서드
    private void UpdateWalkability()
    {
        IsWalkable = GetWalkabilityFromCellType(CellType) | GetWalkabilityFromTileType();
    }

    // CellType에 따른 초기 이동 가능 여부 반환
    private bool GetWalkabilityFromCellType(CellType cellType)
    {
        return cellType switch
        {
            CellType.Floor => true,
            CellType.FloorCenter => true,
            CellType.Path => true,
            CellType.ExpandedPath => true,
            CellType.MainGate => true,
            CellType.SubGate => true,
            CellType.Wall => false,
            CellType.PathWall => false,
            CellType.Empty => false,
            _ => false
        };
    }
    
    private bool GetWalkabilityFromTileType()
    {
        if (_buildObjData == null) return false;
        return _buildObjData.GetTileType() switch
        {
            TileType.Headquarter => true,
            TileType.Road => true,
            TileType.Tree => false,
            TileType.MajorFacility => true,
            TileType.None => false,
            _=> false
        };
    }

    public void SetPlacedObject(PlacedObject placedObject, BuildObjData buildObjData, BuildObjData.Dir dir)
    {
        _placedObject = placedObject;
        _buildObjData = buildObjData;
        _dir = dir;
        UpdateWalkability(); // 건물이 배치되면 이동 가능 여부를 갱신
    }

    public void ClearPlacedObject()
    {
        _placedObject = null;
        _buildObjData = null;
        _dir = BuildObjData.Dir.Down;
        UpdateWalkability(); // 건물이 제거되면 이동 가능 여부를 갱신
    }

    public bool CanBuild()
    {
        return _placedObject == null || _placedObject.IsDefault();
    }

    public TileType? GetTileType() => _buildObjData?.GetTileType();
    public PlacedObject GetPlacedObject() => _placedObject;
    public BuildObjData.Dir GetDirection() => _dir;
    public BuildObjData.Dir GetExitDirection() => _placedObject?.GetActualExitDirection() ?? BuildObjData.Dir.Down;
    public Vector2Int GetEntrancePosition() => _placedObject != null ? _placedObject.GetEntrance() : new Vector2Int(_posX, _posZ);
    public Vector2Int GetExitPosition() => _placedObject != null ? _placedObject.GetExit() : new Vector2Int(_posX, _posZ);

    public override string ToString()
    {
        return $"GridCell({_posX}, {_posZ}) - {CellType}, IsWalkable: {IsWalkable}";
    }
    
    public override bool Equals(object obj)
    {
        return obj is GridCell other && Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}