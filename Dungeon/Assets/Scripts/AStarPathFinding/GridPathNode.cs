using UnityEngine;

// Grid 기반 노드 클래스
public class GridPathNode : IPathNode
{
    public Vector2Int Position { get; }
    public CellType CellType { get; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public IPathNode Parent { get; set; }
    public bool IsWalkable { get; }

    public GridPathNode(Vector2Int position, CellType cellType)
    {
        Position = position;
        CellType = cellType;
        IsWalkable = GetWalkabilityFromCellType(cellType);
        GCost = float.MaxValue; // 초기값은 매우 큰 값으로 설정
    }

    private static bool GetWalkabilityFromCellType(CellType cellType)
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

    public override bool Equals(object obj)
    {
        return obj is GridPathNode other && Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public override string ToString()
    {
        return $"GridNode({Position.x}, {Position.y}) - {CellType}";
    }
}