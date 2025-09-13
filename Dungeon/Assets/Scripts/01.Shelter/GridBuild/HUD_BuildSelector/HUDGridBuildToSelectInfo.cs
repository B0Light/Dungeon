using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class HUDGridBuildToSelectInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buildName;
    [SerializeField] private TextMeshProUGUI buildInfo;
    [SerializeField] private GridBuildingUI gridBuildingUI;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

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
    }

    private void OnDisable()
    {
        GridBuildingSystem.Instance.OnSelectedChanged -= Instance_OnSelectedChanged;
    }
    
    private void Instance_OnSelectedChanged(object sender, System.EventArgs e)
    {
        BuildObjData buildObjData = GridBuildingSystem.Instance.GetPlacedObject();

        _canvasGroup.alpha = buildObjData != null ? 1 : 0;
        Init(buildObjData);
    }
    
    private void Init(BuildObjData buildData)
    {
        if (buildData)
        {
            buildName.text = buildData.itemName;
            buildInfo.text = GetInfoText(buildData);
            
            PlacedObject placedObject = buildData.prefab.gameObject.GetComponent<PlacedObject>();
        
            if(placedObject == null) return;
        
            gridBuildingUI.SetGridLayer(buildData.width, buildData.height, placedObject.entrancePos, placedObject.exitDir, placedObject.exitPos);
        }
        else
        {
            buildName.text = "";
            buildInfo.text = "";
            gridBuildingUI.ClearGrid();
        }
        
    }

    private string GetInfoText(BuildObjData buildData)
    {
        TileType tileType = buildData.GetTileType();
        int cost = buildData.baseFee;
        string description = buildData.itemDescription;
        
        string text = $"[{tileType}]\n수익 : {cost}\n{description}";

        return text;
    }
}
