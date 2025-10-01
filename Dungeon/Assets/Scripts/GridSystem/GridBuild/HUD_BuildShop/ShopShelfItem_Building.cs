public class ShopShelfItem_Building : ShopShelfItem
{
    private BuildObjData _buildObjData;

    public override void Init(ItemData data)
    {
        _buildObjData = data as BuildObjData;
        base.Init(data);
        itemButton.onClick.AddListener(SelectThisItem);
    }
    
    private void SelectThisItem()=>GridBuildingSystem.Instance.SelectToBuild(_buildObjData);
    
    public override int GetItemCategory() => (int)_buildObjData.GetTileType();
}
