using UnityEngine;

public class RoadTile : PlacedObject
{
    [Space(10)]
    // 기본 연결 타입별 프리팹
    [SerializeField] private GameObject defaultPrefab;
    [SerializeField] private GameObject straightPrefab; // 1자 연결 프리팹 (-)
    [SerializeField] private GameObject cornerPrefab;   // ㄱ자 연결 프리팹
    [SerializeField] private GameObject tPrefab;        // ㅗ자 연결 프리팹
    [SerializeField] private GameObject crossPrefab;    // +자 연결 프리팹
    [SerializeField] private string connectionKey = "";
    public void UpdateConnections()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // 상
            new Vector2Int(-1, 0), // 좌
            new Vector2Int(0, -1), // 하
            new Vector2Int(1, 0)   // 우
        };

        // 연결 정보를 나타내는 4비트 문자열 생성
        connectionKey = "";
        

        foreach (var dir in directions)
        {
            Vector2Int checkPos = originPos + dir;
            if (IsRoadAtPosition(checkPos, dir))
            {
                connectionKey += "1";
            }
            else
            {
                connectionKey += "0";
            }
        }

        // 연결 상태에 따라 모델을 업데이트
        UpdateModel(connectionKey);
    }

    private bool IsRoadAtPosition(Vector2Int position, Vector2Int direction)
    {
        GridCell gridObject = GridBuildingSystem.Instance.GetGrid().GetGridObject(position.x, position.y);
        if (gridObject == null)
        {
            return false;
        }

        TileType? tileType = gridObject.GetTileType();
    
        // 일반 도로인 경우 방향 확인 없이 true 반환
        if (tileType == TileType.Road)
        {
            return true;
        }
    
        // 특수 타일(어트랙션, 랜드마크, 본부)의 경우
        if (tileType == TileType.Attraction || tileType == TileType.MajorFacility)
        {
            return IsSpecialTileConnectedToRoad(gridObject, position, direction);
        }
    
        if (tileType == TileType.Headquarter)
        {
            return IsHeadquarterConnectedToRoad(gridObject, position, direction);
        }
    
        return false;
    }

// 어트랙션 또는 랜드마크가 도로와 연결되어 있는지 확인
    private bool IsSpecialTileConnectedToRoad(GridCell gridObject, Vector2Int position, Vector2Int direction)
    {
        // 입구 확인
        if (position == gridObject.GetEntrancePosition())
        {
            BuildObjData.Dir objectDirection = gridObject.GetDirection();
            return objectDirection == ConvertToConnectDirection(direction);
        }
    
        // 출구 확인
        if (position == gridObject.GetExitPosition())
        {
            BuildObjData.Dir objectDirection = gridObject.GetExitDirection();
            return objectDirection == ConvertToConnectDirection(direction);
        }
    
        return false;
    }

// 본부가 도로와 연결되어 있는지 확인
    private bool IsHeadquarterConnectedToRoad(GridCell gridObject, Vector2Int position, Vector2Int direction)
    {
        if (position == gridObject.GetEntrancePosition())
        {
            BuildObjData.Dir objectDirection = gridObject.GetDirection();
            return objectDirection == ConvertToConnectDirection(direction);
        }
    
        return false;
    }
    
    private static BuildObjData.Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return BuildObjData.Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return BuildObjData.Dir.Right;
        if (direction == new Vector2Int(0, -1)) return BuildObjData.Dir.Up;
        if (direction == new Vector2Int(1, 0)) return BuildObjData.Dir.Left;
        return BuildObjData.Dir.Up;
    }

    private void UpdateModel(string connectionKey)
    {
        // 기존 모델 제거
        foreach (Transform child in modelSlot)
        {
            Destroy(child.gameObject);
        }

        // 연결 상태에 따른 모델과 회전 결정
        GameObject prefabToInstantiate = null;
        Quaternion rotation = Quaternion.identity;

        switch (connectionKey)
        {
            case "0000":
                prefabToInstantiate = defaultPrefab;
                break;
            case "0001":
            case "0010":
            case "0100":
            case "1000":
                prefabToInstantiate = straightPrefab;
                rotation = (connectionKey == "0010" || connectionKey == "1000") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                break;
            case "0101": // 좌-우 연결 (1자)
            case "1010": // 상-하 연결 (1자)
                prefabToInstantiate = straightPrefab;
                rotation = (connectionKey == "1010") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                break;

            case "1001": // 상-우 연결 (ㄱ자)
            case "1100": // 상-좌 연결 (ㄱ자)
            case "0011": // 좌-하 연결 (ㄱ자)
            case "0110": // 하-우 연결 (ㄱ자)
                prefabToInstantiate = cornerPrefab;
                rotation = GetCornerRotation(connectionKey);
                break;

            case "1110": // 상-좌-우 연결 (ㅗ자)
            case "1011": // 상-하-좌 연결 (ㅗ자)
            case "0111": // 좌-하-우 연결 (ㅗ자)
            case "1101": // 상-하-우 연결 (ㅗ자)
                prefabToInstantiate = tPrefab;
                rotation = GetTRotation(connectionKey);
                break;

            case "1111": // 상-하-좌-우 연결 (+자)
                prefabToInstantiate = crossPrefab;
                break;

            default:
                prefabToInstantiate = defaultPrefab;
                break; // 연결 없음
        }

        // 새 모델 생성 및 적용
        if (prefabToInstantiate != null)
        {
            GameObject iTile = Instantiate(prefabToInstantiate, modelSlot);
            iTile.transform.localPosition = Vector3.zero;
            iTile.transform.localRotation = rotation;
        }
    }

    private Quaternion GetCornerRotation(string connectionKey)
    {
        switch (connectionKey)
        {
            case "0110": return Quaternion.Euler(0, 0, 0);   
            case "1100": return Quaternion.Euler(0, 90, 0); 
            case "1001": return Quaternion.Euler(0, 180, 0); 
            case "0011": return Quaternion.Euler(0, 270, 0);  
            default: return Quaternion.identity;
        }
    }

    private Quaternion GetTRotation(string connectionKey)
    {
        switch (connectionKey)
        {
            case "0111": return Quaternion.Euler(0, 0, 0); 
            case "1110": return Quaternion.Euler(0, 90, 0);   
            case "1101": return Quaternion.Euler(0, 180, 0);  
            case "1011": return Quaternion.Euler(0, 270, 0); 
            default: return Quaternion.identity;
        }
    }

}
