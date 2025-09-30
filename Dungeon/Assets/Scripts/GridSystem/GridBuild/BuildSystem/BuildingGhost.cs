using System.Collections;
using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    [SerializeField] private Material ghostMaterialEnable;
    [SerializeField] private Material ghostMaterialDisable;
    private Transform _visual;
    private Material _curOriginMat;
    private Material _curMat;
    private void Start() 
    {
        RefreshVisual();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForGridBuildingSystem());
    }
    
    private IEnumerator WaitForGridBuildingSystem()
    {
        // GridBuildingSystem.Instance가 null이 아닌지 확인
        while (GridBuildingSystem.Instance == null)
        {
            yield return null; // 매 프레임 기다림
        }

        // GridBuildingSystem.Instance가 설정되었을 때 이벤트 등록
        GridBuildingSystem.Instance.OnObjectPlaced += Instance_OnSelectedChanged;
        GridBuildingSystem.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
    }

    private void OnDisable()
    {
        GridBuildingSystem.Instance.OnObjectPlaced -= Instance_OnSelectedChanged;
        GridBuildingSystem.Instance.OnSelectedChanged -= Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(object sender, System.EventArgs e) 
    {
        RefreshVisual();
    }

    private void LateUpdate() 
    {
        if(GridBuildingSystem.Instance.GetPlacedObject() == null) return;
        Vector3 targetPosition = GridBuildingSystem.Instance.GetMouseWorldSnappedPosition();
        targetPosition.y = 1f;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, GridBuildingSystem.Instance.GetPlacedObjectRotation(), Time.deltaTime * 15f);

        if (_visual)
        {
            MeshRenderer[] mrs = _visual.GetComponentsInChildren<MeshRenderer>();
            Material selectMat = GridBuildingSystem.Instance.CheckCanBuildAtPos() ? _curOriginMat : ghostMaterialDisable;
            foreach (var mr in mrs)
            {
                mr.material = selectMat;
            }
        }
    }

    private void RefreshVisual() 
    {
        if (_visual != null) 
        {
            Destroy(_visual.gameObject);
            _visual = null;
        }

        BuildObjData placedObjectData = GridBuildingSystem.Instance.GetPlacedObject();

        if (placedObjectData != null) 
        {
            _visual = Instantiate(placedObjectData.prefab, Vector3.zero, Quaternion.identity);
            _visual.parent = transform;
            _visual.localPosition = Vector3.zero;
            _visual.localEulerAngles = Vector3.zero;
            MeshRenderer[] mrs = _visual.GetComponentsInChildren<MeshRenderer>();
            
            _curOriginMat = (GridBuildingSystem.Instance.CanBuildObject())
                ? ghostMaterialEnable
                : ghostMaterialDisable;
            foreach (var mr in mrs)
            {
                mr.material = _curOriginMat;
            }
            SetLayerRecursive(_visual.gameObject, 15);
        }
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer) 
    {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform) 
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

}
