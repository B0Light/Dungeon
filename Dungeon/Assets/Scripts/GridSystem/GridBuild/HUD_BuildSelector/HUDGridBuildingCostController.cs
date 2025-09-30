using System.Collections;
using UnityEngine;

public class HUDGridBuildingCostController : MonoBehaviour
{
    [SerializeField] private GameObject costPrefab;
    [SerializeField] private Transform costItemSlot;
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
        while (!GridBuildingSystem.Instance)
        {
            yield return null; // 한 프레임 대기
        }
    }
    private void OnEnable()
    {
        StartCoroutine(BindSelectBuilding());
    }
    
    private IEnumerator BindSelectBuilding()
    {
        yield return StartCoroutine(WaitForDataLoad());
        GridBuildingSystem.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
        GridBuildingSystem.Instance.OnObjectPlaced += Instance_OnSelectedChanged;
    }

    private void OnDisable()
    {
        GridBuildingSystem.Instance.OnSelectedChanged -= Instance_OnSelectedChanged;
        GridBuildingSystem.Instance.OnObjectPlaced -= Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(object sender, System.EventArgs e)
    {
        ItemData item = GridBuildingSystem.Instance.GetPlacedObject();
        
        DeleteAllChildren(costItemSlot);
        if (item == null) return;
        
        
        foreach (var costItemPair in item.GetCostDict())
        {
            GameObject spawnedCostItem = Instantiate(costPrefab, costItemSlot);
                
            spawnedCostItem.GetComponent<ShopCostItem>()?.Init(costItemPair.Key, costItemPair.Value);
        }
    }
    
    private void DeleteAllChildren(Transform parentTransform)
    {
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentTransform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
