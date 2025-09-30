using System;
using UnityEngine;

public class GridXZ<GridCell>
{
    private readonly int _width;
    private readonly int _height;
    private readonly float _cellSize;
    private readonly Vector3 _originPosition;
    private readonly GridCell[,] _gridArray;
    
    public GridXZ(int width, int height, float cellSize, Vector3 originPosition,
        Func<int, int, GridCell> createGridObject)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _originPosition = originPosition;
        _gridArray = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                _gridArray[x, z] = createGridObject(x, z);
            }
        }
    }

    public GridCell[,] GetAllGridObjects() => _gridArray;
    
    public float GetCellSize() => _cellSize;

    public Vector3 GetWorldPosition(int x, int z) => new Vector3(x, 0, z) * _cellSize + _originPosition;

    public Vector3 GetWorldPosition(Vector2Int pos) => new Vector3(pos.x, 0, pos.y) * _cellSize + _originPosition;
    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
        z = Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize);
    }

    public GridCell GetGridObject(int x, int z) => IsValidGridPosition(x, z) ? _gridArray[x, z] : default;
    
    private bool IsValidGridPosition(int x, int z) => x >= 0 && z >= 0 && x < _width && z < _height;
}
