using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridBuildingSystem : MonoBehaviour
{ 
    public static GridBuildingSystem Instance { get; private set; }
    private static GridBuildingSystem _instance;
    
   private BuildObjData _objectToPlace;

    private GridXZ<GridObject> _grid;
    private BuildObjData.Dir _dir = BuildObjData.Dir.Down;
    private readonly int _gridWidth = 7;
    private readonly int _gridLength = 9;
    private readonly int _cellSize = 5;

    [SerializeField, Range(0,1f)] private float depreciation = 0.7f;

    [Space(10)] 
    [ReadOnly] private Vector2Int entracnePos = new Vector2Int(3, 0);
    [ReadOnly] private Vector2Int headquarterPos = new Vector2Int(4, 1);

    [Space(10)] 
    [SerializeField] private GameObject selector;
    [SerializeField] private MeshRenderer selectorMeshRenderer;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material deleteMat;
    
    private bool _isActive = false;
    private readonly Variable<bool> _isDeleteMode = new Variable<bool>(false);
    private bool _isDragging = false; // 드래그 상태 추적
    private Vector2Int _lastPlacedPosition; // 마지막 배치 위치
    
    public List<SaveBuildingData> SaveBuildingDataList { get; private set; }

    // Attraction Entrance Position List -> For NPC
    public List<Vector2Int> AttractionEntrancePosList { get; private set; }

    public event EventHandler OnSelectedChanged;
    public event EventHandler OnObjectPlaced;

    private void Awake()
    {
        Instance = this;
        _grid = new GridXZ<GridObject>(
            _gridWidth,
            _gridLength,
            _cellSize,
            transform.position,
            (GridXZ<GridObject> g, int x, int z) => new GridObject(g, x, z)
        );
        _lastPlacedPosition = new Vector2Int(-1, -1); // 초기화: 유효하지 않은 위치
    }

    private void OnEnable()
    {
        _isDeleteMode.OnValueChanged += SetSelectorMat;
    }
    
    private void OnDisable()
    {
        _isDeleteMode.OnValueChanged -= SetSelectorMat;
    }
    
    private void SetSelectorMat(bool newValue)
    {
        selectorMeshRenderer.material = newValue ? deleteMat : baseMat;
    }

    private void Start()
    {
        AttractionEntrancePosList = new List<Vector2Int>();
        LoadDefaultObject();
        LoadSaveBuildingData();
        LoadDefaultTiles();
    }

    private void LoadDefaultObject()
    {
        LoadEntrance();
        LoadDefaultRoad();
        LoadHeadquarter();
    }

    private void LoadDefaultTiles()
    {
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridLength; j++)
            {
                SetDefaultTile(i,j);
            }
        }
        _objectToPlace = null;
    }

    private void SetDefaultTile(int x, int y)
    {
        _objectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(0);
        if(_objectToPlace == null) return;
        PlaceTile(x,y, BuildObjData.Dir.Down);
        _objectToPlace = null;
    }

    private void LoadEntrance()
    {
        _objectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(1);
        if(_objectToPlace == null) return;
        var placedObject = PlaceTile(entracnePos.x,entracnePos.y,BuildObjData.Dir.Down, 0,true);
        AttractionEntrancePosList.Add(placedObject.GetEntrance());
        _objectToPlace = null;
    }

    public Vector2Int GetEntrancePos() => entracnePos;
    
    private void LoadHeadquarter()
    {
        _objectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(2);
        if(_objectToPlace == null) return;
        var placedObject = PlaceTile(headquarterPos.x,headquarterPos.y,BuildObjData.Dir.Left,
            WorldSaveGameManager.Instance.currentGameData.shelterLevel,true);
        AttractionEntrancePosList.Add(placedObject.GetEntrance());
        _objectToPlace = null;
    }

    public Vector2Int GetHeadquarterPos() => headquarterPos;
    
    private void LoadDefaultRoad()
    {
        _objectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(3);
        if(_objectToPlace == null) return;
        PlaceTile(3,1,BuildObjData.Dir.Down, 0,true);
        _objectToPlace = null;
    }
    
    private void LoadSaveBuildingData()
    {
        SaveBuildingDataList = new List<SaveBuildingData>();
        foreach (var saveData in WorldSaveGameManager.Instance.currentGameData.buildings)
        {
            int sX = saveData.x;
            int sZ = saveData.y;
            _objectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(saveData.code);
            if(_objectToPlace == null) continue;
            BuildObjData.Dir dir = _objectToPlace.GetTileType() == TileType.Road ? BuildObjData.Dir.Down : (BuildObjData.Dir)saveData.dir;
            PlaceTile(sX,sZ, dir, saveData.level);
        }
        _objectToPlace = null;
    }

    private void Update()
    {
        if (!_isActive)
        {
            _objectToPlace = null;
            selector.SetActive(false);
            return;
        }
        
        selector.SetActive(GetPlacedObject() == null);
        if (GetPlacedObject() == null)
        {
            Vector3 targetPosition = GetMouseWorldSnappedPosition();
            _grid.GetXZ(targetPosition, out int x, out int z);
            
            GridObject gridObject = _grid.GetGridObject(x, z);
            PlacedObject placedObject = gridObject?.GetPlacedObject();

            if (placedObject)
            {
                BuildObjData obj = placedObject.GetBuildObjData();
                var dir = placedObject.GetDir();
                selector.transform.localScale = new Vector3(obj.GetWidth(dir),1,obj.GetHeight(dir));

                targetPosition = _grid.GetWorldPosition(placedObject.GetOriginPos());
            }
            targetPosition.y = 0.5f;
            selector.transform.position = Vector3.Lerp(selector.transform.position, targetPosition, Time.deltaTime * 15f);
        }
        

        if (Input.GetMouseButtonDown(1))
        {
            SelectToBuild(null);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                _isDragging = false;
                return;
            }

            if (_objectToPlace != null)
            {
                _isDragging = true;
            }
            else
            {
                _isDragging = false;
                if (_isDeleteMode.Value)
                {
                    RemoveObjectAtMousePosition();
                }
                else
                {
                    SelectObjectAtMousePosition();
                }
            }
        }

        if (Input.GetMouseButton(0) && _isDragging)
        {
            PlaceObjectAtMousePositionIfNeeded();
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _isDragging = false;
            _lastPlacedPosition = new Vector2Int(-1, -1); // 드래그 종료 시 마지막 위치 초기화
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            _dir = BuildObjData.GetNextDir(_dir);
        }

        if (_objectToPlace == null || _objectToPlace?.GetTileType() == TileType.Road)
        {
            _dir = BuildObjData.Dir.Down;
        }
    }

    private void PlaceObjectAtMousePositionIfNeeded()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        _grid.GetXZ(mousePosition, out int x, out int z);
        Vector2Int currentGridPosition = new Vector2Int(x, z);

        // 중복 배치 방지
        if (currentGridPosition == _lastPlacedPosition) return;

        if (CheckCanBuildAtPos(x,z))
        {
            if (CheckItemInInventory(_objectToPlace) && SpendItemInInventory(_objectToPlace))
            {
                PlaceTile(x, z, _dir);
                _lastPlacedPosition = currentGridPosition; // 마지막 배치 위치 갱신
            }
            else
            {
                Debug.Log("No Item");
            }
        }
        else
        {
            Debug.Log("Can Not Build Here");
        }
    }

    private bool CheckCanBuildAtPos(int x, int z)
    {
        return IsPlacementValid(x, z) && CanBuildAtPos(_objectToPlace.GetGridPositionList(new Vector2Int(x, z), _dir));
    }

    public bool CheckCanBuildAtPos()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        _grid.GetXZ(mousePosition, out int x, out int z);
        return IsPlacementValid(x, z) && CanBuildAtPos(_objectToPlace.GetGridPositionList(new Vector2Int(x, z), _dir));
    }

    private bool CheckItemInInventory(ItemData buyObject)
    {
        return WorldPlayerInventory.Instance.CheckItemInInventoryToChangeItem(buyObject);
    }


    private bool SpendItemInInventory(ItemData buyObject)
    {
        return WorldPlayerInventory.Instance.SpendItemInInventory(buyObject);
    }


    private bool CanBuildAtPos(List<Vector2Int> gridPositionList)
    {
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _grid.GetGridObject(gridPosition.x, gridPosition.y);
            if (gridObject == null || !gridObject.CanBuild())
            {
                return false;
            }
        }
        return true;
    }
    
    private PlacedObject PlaceTile(int x, int z, BuildObjData.Dir dir, int level = 0, bool isIrremovable = false)
    {
        var gridPositionList = _objectToPlace.GetGridPositionList(new Vector2Int(x, z), dir);

        if(!CanBuildAtPos(gridPositionList)) return null;
        
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _grid.GetGridObject(gridPosition.x, gridPosition.y);
            gridObject.GetPlacedObject()?.DestroySelf();
            gridObject.ClearPlacedObject();
        }
        
        PlacedObject placedObject = BuildTile(x, z, dir, level, isIrremovable);
        if(_objectToPlace.itemCode >= 100)
        {
            SaveBuildingDataList.Add(new SaveBuildingData(x, z, _objectToPlace.itemCode, (int)dir, level));
        }
        if(_objectToPlace.itemCode >= 300)
        {
            AttractionEntrancePosList.Add(placedObject.GetEntrance());
        }

        return placedObject;
    }

    private PlacedObject BuildTile(int x, int z, BuildObjData.Dir dir, int level = 0, bool isIrremovable = false)
    {
        Vector2Int rotationOffset = _objectToPlace.GetRotationOffset(dir);
        Vector3 placedObjectWorldPosition = _grid.GetWorldPosition(x, z) +
                                            new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.GetCellSize();

        PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(x, z), dir, _objectToPlace, level, isIrremovable);

        var gridPositionList = _objectToPlace.GetGridPositionList(new Vector2Int(x, z), dir);
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            SetObjectAtGridPosition(gridPosition, placedObject, dir);
        }

        IsUpdateSurroundingRoad(placedObject);

        OnObjectPlaced?.Invoke(this, EventArgs.Empty);
        return placedObject;
    }
    
    public bool TryUpgrade(PlacedObject placedObject)
    {
        BuildObjData buildObjData = placedObject.GetBuildObjData();

        if (placedObject.GetLevel() >= buildObjData.maxLevel)
        {
            Debug.LogWarning("최고 레벨 ");
            return false;
        }

        if (WorldPlayerInventory.Instance.TrySpend(placedObject.GetUpgradeCost()))
        {
            UpgradeTile(placedObject);
            return true;
        }
        // 실패시 처리 
        Debug.LogWarning("[비용 부족] 비용 : " + placedObject.GetUpgradeCost());
        return false;
    }
    
    private void UpgradeTile(PlacedObject placedObject)
    {
        Vector2Int originPos = placedObject.GetOriginPos();
        int itemCode = placedObject.GetBuildObjData().itemCode;
        int direction = (int)placedObject.GetDir();
        int previousLevel = placedObject.GetLevel();

        // 업그레이드 실행
        placedObject.UpgradeTile();

        var oldData = new SaveBuildingData(originPos.x, originPos.y, itemCode, direction, previousLevel);
        var newData = new SaveBuildingData(originPos.x, originPos.y, itemCode, direction, previousLevel + 1);

        if (SaveBuildingDataList.Remove(oldData))
        {
            SaveBuildingDataList.Add(newData);
        }
        else
        {
            Debug.LogWarning("기존 저장 데이터가 제거되지 않았습니다.");
        }
    }
    
    private void IsUpdateSurroundingRoad(PlacedObject placedObject)
    {
        TileType curTileType = _objectToPlace.GetTileType();
        if (curTileType == TileType.Road)
        {
            UpdateSurroundingRoads(placedObject.GetEntrance());
        }
        if (curTileType == TileType.Attraction || curTileType == TileType.MajorFacility)
        {
            UpdateSurroundingRoads(placedObject.GetExit());
            UpdateSurroundingRoads(placedObject.GetEntrance());
        }
        if (curTileType == TileType.Headquarter)
        {
            UpdateSurroundingRoads(placedObject.GetEntrance());
        }
    }

    private void RemoveObjectAtMousePosition()
    {
        // 손에 배치할 타일이 있으면 타일 제거 불가 
        if(_objectToPlace) return;
        
        PlacedObject placedObject = GetObjectAtMousePosition();
        
        if (placedObject != null && placedObject.Irremovable == false)
        {
            int itemCode = placedObject.GetBuildObjData().itemCode;
            // 저장 데이터에서 삭제
            if (SaveBuildingDataList.Remove(new SaveBuildingData(placedObject.GetOriginPos().x, placedObject.GetOriginPos().y,
                    itemCode, (int)placedObject.GetDir(), placedObject.GetLevel())))
            {
                if (placedObject.GetLevel() > 0)
                {
                    WorldPlayerInventory.Instance.balance.Value += Mathf.RoundToInt(placedObject.GetTotalUpgradeCost() * depreciation);
                }
            }
            
            if(itemCode >= 300)
                AttractionEntrancePosList.Remove(placedObject.GetEntrance());
            
            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

            placedObject.DestroySelf();
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                ClearObjectAtGridPosition(gridPosition);
            }

            foreach (Vector2Int gridPosition in gridPositionList)
            {
                UpdateSurroundingRoads(gridPosition);
                SetDefaultTile(gridPosition.x,gridPosition.y);
            }
        }
    }

    private void SelectObjectAtMousePosition()
    {
        if (_objectToPlace) return;
        PlacedObject placedObject = GetObjectAtMousePosition();
    
        if (placedObject is RevenueFacilityTile attractionTile)
        {
            BuildingManager.Instance.OpenBuildPopUpHUD(attractionTile);
        }
    }

    private PlacedObject GetObjectAtMousePosition()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        _grid.GetXZ(mousePosition, out int x, out int z);

        GridObject gridObject = _grid.GetGridObject(x, z);
        return gridObject?.GetPlacedObject();
    }
    
    // _grid 특정 위치에 객체 배치
    private void SetObjectAtGridPosition(Vector2Int position, PlacedObject placedObject, BuildObjData.Dir dir)
    {
        var gridObject = _grid.GetGridObject(position.x, position.y);
        gridObject?.SetPlacedObject(placedObject, _objectToPlace, dir); // BuildObjData 저장
    }

    // 특정 위치의 객체 제거
    private void ClearObjectAtGridPosition(Vector2Int gridPosition)
    {
        var gridObject = _grid.GetGridObject(gridPosition.x, gridPosition.y);
        if (gridObject != null)
        {
            gridObject.ClearPlacedObject();
        }
    }

    private void UpdateSurroundingRoads(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 상
            new Vector2Int(0, -1),  // 하
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(1, 0)    // 우
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = position + dir;
            var neighborObject = _grid.GetGridObject(neighborPos.x, neighborPos.y)?.GetPlacedObject();

            if (neighborObject is RoadTile neighborRoad)
            {
                neighborRoad.UpdateConnections(); // 주변 도로의 연결 상태 업데이트
            }
        }
    }

    
    private bool IsPlacementValid(int x, int z)
    {
        int objectWidth = _objectToPlace.GetWidth(_dir);
        int objectLength = _objectToPlace.GetHeight(_dir);

        return x >= 0 && z >= 0 &&
               x + objectWidth <= _gridWidth &&
               z + objectLength <= _gridLength;
    }
    
    public void SelectToBuild(BuildObjData buildData)
    {
        _isDeleteMode.Value = false;
        _objectToPlace = buildData;
        OnSelectedChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void SetActive(bool isActive)
    {
        _isActive = isActive;
    }
    
    public Vector3 GetMouseWorldSnappedPosition() {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        _grid.GetXZ(mousePosition, out int x, out int z);

        Vector3 placedObjectWorldPosition = _grid.GetWorldPosition(x, z);
        if (_objectToPlace != null) {
            Vector2Int rotationOffset = _objectToPlace.GetRotationOffset(_dir);
            placedObjectWorldPosition += new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.GetCellSize();
        } 
        
        return placedObjectWorldPosition;
    }
    
    public Quaternion GetPlacedObjectRotation() {
        if (_objectToPlace != null) {
            return Quaternion.Euler(0, _objectToPlace.GetRotationAngle(_dir), 0);
        } else {
            return Quaternion.identity;
        }
    }

    public bool CanBuildObject()
    {
        return _objectToPlace && CheckItemInInventory(_objectToPlace);
    }

    public void SetDeleteMode()
    {
        SelectToBuild(null);
        
        _isDeleteMode.Value = (BuildingManager.Instance.shelterManager.IsVisitorInShelter() == false);
    }
    
    public BuildObjData GetPlacedObject() => 
        _objectToPlace?.GetTileCategory() == TileCategory.Headquarter ? null : _objectToPlace;

    
    public GridXZ<GridObject> GetGrid() => _grid;
}