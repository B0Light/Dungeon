using System.Collections.Generic;
using UnityEngine;

public class DynamicGridXZ<T> : GridBase<T>
{
    private Dictionary<Vector2Int, T> _data;

    public Vector2Int Offset { get; set; }

    public DynamicGridXZ(Vector2Int size, Vector2Int offset) : base(size.x, size.y, 1f, new Vector3(offset.x, 0, offset.y))
    {
        Offset = offset;
        _data = new Dictionary<Vector2Int, T>();
    }

    public bool InBounds(Vector2Int pos)
    {
        // GridBase의 Width와 Height를 사용하여 경계 검사
        return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    }

    public override T GetGridObject(int x, int z)
    {
        Vector2Int pos = new Vector2Int(x, z);
        if (_data.TryGetValue(pos + Offset, out var value))
        {
            return value;
        }
        else
        {
            return default(T);
        }
    }

    public override bool IsValidGridPosition(int x, int z) => InBounds(new Vector2Int(x, z));

    public T this[int x, int y]
    {
        get
        {
            return this[new Vector2Int(x, y)];
        }
        set
        {
            this[new Vector2Int(x, y)] = value;
        }
    }

    public T this[Vector2Int pos]
    {
        get
        {
            if (_data.TryGetValue(pos + Offset, out var value))
            {
                return value;
            } else
            {
                return default(T); // 기본값 반환, 값이 없을 경우
            }
        }
        set
        {
            _data[pos + Offset] = value;
        }
    }

    public IEnumerable<Vector2Int> GetAllPositions()
    {
        foreach (var key in _data.Keys)
        {
            yield return key - Offset;
        }
    }
}
