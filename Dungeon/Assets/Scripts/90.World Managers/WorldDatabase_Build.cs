using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldDatabase_Build : Singleton<WorldDatabase_Build>
{
    public bool IsDataLoaded { get; private set; }
    
    private readonly List<BuildObjData> _allBuildObjDataList = new List<BuildObjData>();
    private readonly Dictionary<ItemTier, List<BuildObjData>> _buildObjByLevel = new Dictionary<ItemTier, List<BuildObjData>>();
    private readonly List<Sprite> _defaultCategoryIcon = new List<Sprite>();
    
    protected override void Awake()
    {
        base.Awake();
        IsDataLoaded = false;
        LoadData();
        ClassifyData();
        IsDataLoaded = true;
    }

    private void LoadData()
    {
        BuildObjData[] buildingObjects = Resources.LoadAll<BuildObjData>("Building");
        foreach (var unit in buildingObjects)
        {
            if(unit.isForbidden) continue;
            _allBuildObjDataList.Add(unit);
        }
        
        Sprite[] defaultIcons = Resources.LoadAll<Sprite>("Building/99_DefaultIcon");
        foreach (var icon in defaultIcons)
        {
            _defaultCategoryIcon.Add(icon);
        }
    }

    private void ClassifyData()
    {
        foreach (var buildObj in _allBuildObjDataList)
        {
            var tier = buildObj.itemTier;
        
            if (!_buildObjByLevel.TryGetValue(tier, out List<BuildObjData> buildObjList))
            {
                buildObjList = new List<BuildObjData>();
                _buildObjByLevel[tier] = buildObjList;
            }
        
            buildObjList.Add(buildObj);
        }
    }
    
    public BuildObjData GetBuildingByID(int id) => 
        _allBuildObjDataList.FirstOrDefault(buildObjData => buildObjData.itemCode == id);

    public Sprite GetCategoryIcon(TileCategory id) => _defaultCategoryIcon[(int)id];
    
    public IReadOnlyList<BuildObjData> GetBuildingsByTierReadOnly(ItemTier tier)
    {
        return _buildObjByLevel.TryGetValue(tier, out List<BuildObjData> buildObjList) 
            ? buildObjList.AsReadOnly() 
            : new List<BuildObjData>().AsReadOnly();
    }
    
    public IReadOnlyList<BuildObjData> GetBuildingsUpToTierReadOnly(ItemTier maxTier)
    {
        List<BuildObjData> result = new List<BuildObjData>();
        
        foreach (var kvp in _buildObjByLevel)
        {
            if (kvp.Key <= maxTier)
            {
                result.AddRange(kvp.Value);
            }
        }
        
        return result.AsReadOnly();
    }
}