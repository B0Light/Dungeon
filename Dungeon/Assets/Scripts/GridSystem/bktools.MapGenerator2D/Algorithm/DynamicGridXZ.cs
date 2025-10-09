using System.Collections.Generic;
using UnityEngine;

public class DynamicGridXZ<T> : GridBase<T>
{
    private Dictionary<Vector2Int, T> _data;

    public DynamicGridXZ(Vector2Int size) : base(size.x, size.y, 1f, Vector3.zero)
    {
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
        if (_data.TryGetValue(pos, out var value))
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
            if (_data.TryGetValue(pos, out var value))
            {
                return value;
            } else
            {
                return default(T); // 기본값 반환, 값이 없을 경우
            }
        }
        set
        {
            _data[pos] = value;
        }
    }
}
