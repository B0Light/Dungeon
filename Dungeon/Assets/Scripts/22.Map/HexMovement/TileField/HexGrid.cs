using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexGrid : Singleton<HexGrid>
{
    public static float xOffset = 4.325f, zOffset = 5.0f; //  yOffset = 0.5f,

    private Dictionary<HexCoordinate, Hex> hexTileDict = new Dictionary<HexCoordinate, Hex>();
    Dictionary<HexCoordinate, List<HexCoordinate>> hexTileNeighboursDict = new Dictionary<HexCoordinate, List<HexCoordinate>>();

    private List<Hex> _emptyHexTiles = new List<Hex>();

    static class Direction {
        public static List<HexCoordinate> directionsOffsetOdd = new List<HexCoordinate>
    {
        new HexCoordinate( 0, 1), //N
        new HexCoordinate( 1, 0), //E1
        new HexCoordinate( 1, -1), //E2
        new HexCoordinate( 0, -1), //S
        new HexCoordinate(-1, -1), //W1
        new HexCoordinate(-1, 0), //W2
    };

        public static List<HexCoordinate> directionsOffsetEven = new List<HexCoordinate>
    {
        new HexCoordinate( 0, 1), //N
        new HexCoordinate( 1, 1), //E1
        new HexCoordinate( 1, 0), //E2
        new HexCoordinate( 0, -1), //S
        new HexCoordinate(-1, 0), //W1
        new HexCoordinate(-1, 1), //W2
    };

        public static List<HexCoordinate> GetDirectionList(int x)
            => x % 2 == 0 ? directionsOffsetEven : directionsOffsetOdd;
    }

    public void AddTile(Hex hex) {
        hexTileDict[hex.HexCoords] = hex;
    }

    public Hex GetTileAt(HexCoordinate hexCoordinate)
    {
        hexTileDict.TryGetValue(hexCoordinate, out Hex result);
        return result;

    }
    
    public HexCoordinate GetClosestHex(Vector3 worldposition)
    {
        worldposition.y = 0;
        return HexCoordinate.ConvertFromVector3(worldposition);
    }
    
    public List<HexCoordinate> GetNeighboursFor(HexCoordinate hexCoordinates)
    {
        if (hexTileDict.ContainsKey(hexCoordinates) == false)
            return new List<HexCoordinate>();

        if (hexTileNeighboursDict.ContainsKey(hexCoordinates))
            return hexTileNeighboursDict[hexCoordinates];

        hexTileNeighboursDict.Add(hexCoordinates, new List<HexCoordinate>());

        foreach (HexCoordinate pos in HexCoordinate.GetDirectionList(hexCoordinates))
        {
            if (hexTileDict.ContainsKey(pos))
            {
                hexTileNeighboursDict[hexCoordinates].Add(pos);
            }
            
        }
        return hexTileNeighboursDict[hexCoordinates];
    }
}
