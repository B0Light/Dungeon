using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[SerializeField]
public class BuildObjData : ItemData
{
    [System.Serializable]
    public struct UpgradeStage
    {
        public Transform prop;
        public int cost;
    }
    public enum Dir
    {
        Down,
        Left,
        Up,
        Right,
    }

    public bool isForbidden = false;
    public Transform prefab;
    
    [Space(10)]
    [SerializeField] private TileCategory tileCategory;
    [SerializeField] private TileType tileType;
    public int maxLevel = 10;
    public int baseFee = 100;
    
    public List<UpgradeStage> upgradeStages = new List<UpgradeStage>();

    // Direction-related utilities
    public static Dir GetNextDir(Dir dir) => (Dir)(((int)dir + 1) % 4);

    public int GetRotationAngle(Dir dir) => ((int)dir * 90) % 360;

    public Vector2Int GetRotationOffset(Dir dir) => dir switch
    {
        Dir.Left => new Vector2Int(0, width),
        Dir.Up => new Vector2Int(width, height),
        Dir.Right => new Vector2Int(height, 0),
        _ => Vector2Int.zero, // Dir.Down
    };

    public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir)
    {
        var gridPositionList = new List<Vector2Int>();
        int primary = GetWidth(dir);
        int secondary = GetHeight(dir);

        for (int x = 0; x < primary; x++)
        {
            for (int y = 0; y < secondary; y++)
            {
                gridPositionList.Add(offset + new Vector2Int(x, y));
            }
        }

        return gridPositionList;
    }


    public int GetWidth(Dir dir) => dir is Dir.Down or Dir.Up ? width : height;

    public int GetHeight(Dir dir) => dir is Dir.Down or Dir.Up ? height : width;

    public TileType GetTileType() => tileType;
    
    public TileCategory GetTileCategory() => tileCategory;
}