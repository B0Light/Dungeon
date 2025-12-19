using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Serialization;

public class PlayerCameraController : Singleton<PlayerCameraController>
{
    private bool _enable = false;
    [HideInInspector] public PlayerManager playerManager;
    [SerializeField] private Camera mainCamera;

    private Transform _playerTarget;
    
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera vCam; 

    [Header("Occlusion Settings")] 
    private readonly float _occlusionRadius = 5.0f;
    [SerializeField] private bool hideOption = true;
    [SerializeField] private LayerMask occlusionLayer; 

    [Header("Material Replacement")]
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private Material replacementMaterial;
    

    [Header("Cam Mode Culling Mask")]
    [SerializeField] private LayerMask layerExploration;
    [SerializeField] private LayerMask layerBuild;
    
    [Header("FOG")]
    [SerializeField] private FieldOfView_Sight fieldOfViewSight;
    [SerializeField] private FieldOfView_Sight playerSight;
    [SerializeField] private Vector3 offset = new Vector3(0,0.1f,0);
    
    // 딕셔너리에 원래 재질 정보 저장
    private readonly Dictionary<Renderer, Material[]> _occludedRenderers = new Dictionary<Renderer, Material[]>();
    
    // 최적화: Physics.RaycastNonAlloc()을 위한 배열 사전 할당
    private const int MAX_HITS = 100;
    private RaycastHit[] _raycastHits = new RaycastHit[MAX_HITS];
    
    // 방향 전환 
    private Vector2 _mousePositionInput = Vector2.zero;
    private Vector3 _curOrthographicDirection;
    private Vector3 _curDir;
    private float _targetSize = 3.5f;
    
    public void Update()
    {
        if(!_enable) return;
        HandleOcclusion();
        UpdateSight();
        UpdateDir();
        UpdateCamZoom();
    }

    private void HandleOcclusion()
    {
        if(!hideOption) return;

        var currentOccludedRenderers = new HashSet<Renderer>();

        Vector3 origin = mainCamera.transform.position;
        Vector3 direction = (_playerTarget.position - origin).normalized;
        float distance = Vector3.Distance(origin, _playerTarget.position) - 6f;

        // Physics.RaycastNonAlloc 대신 SphereCastNonAlloc 사용
        int hitCount = Physics.SphereCastNonAlloc(origin, _occlusionRadius, direction, _raycastHits, distance, occlusionLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _raycastHits[i];
            var rds = hit.collider.GetComponentsInChildren<Renderer>();

            var targetMat = hit.collider.CompareTag("Transparent") ? transparentMaterial : replacementMaterial;
            foreach (var rd in rds)
            {
                if (rd != null)
                {
                    currentOccludedRenderers.Add(rd);

                    if (!_occludedRenderers.ContainsKey(rd))
                    {
                        _occludedRenderers[rd] = rd.sharedMaterials;
                        Material[] newMaterials = new Material[rd.sharedMaterials.Length];
                        for (int j = 0; j < newMaterials.Length; j++)
                        {
                            newMaterials[j] = targetMat;
                        }
                        rd.materials = newMaterials;
                    }
                }
            }
        }

        var renderersToRestore = new List<Renderer>();
        foreach(var rd in _occludedRenderers.Keys)
        {
            if(!currentOccludedRenderers.Contains(rd))
                renderersToRestore.Add(rd);
        }

        foreach(var rd in renderersToRestore)
        {
            rd.materials = _occludedRenderers[rd];
            _occludedRenderers.Remove(rd);
        }
    }
    
    // Orthographic 상황에서 방향지시 -> 2d 시야 확보
    private void UpdateSight()
    {
        Vector3 screenPlayerPos = mainCamera.WorldToScreenPoint(playerManager.transform.position + offset);
        Vector2 directionOnScreen = (Vector2)_mousePositionInput - (Vector2)screenPlayerPos;
        Vector3 worldDirection = new Vector3(directionOnScreen.x, 0, directionOnScreen.y);
        if (worldDirection.sqrMagnitude > 0.001f)
        {
            _curOrthographicDirection = worldDirection.normalized;
            var pos = playerManager.transform.position + offset;
            fieldOfViewSight.SetAimDirection(_curOrthographicDirection);
            fieldOfViewSight.SetOrigin(pos);
            playerSight.SetOrigin(pos);
        }
    }
    
    // 3D 방식에서 방향지시 -> 3d 캐릭터가 바라보는 방향 
    private void UpdateDir()
    {
        Ray ray = mainCamera.ScreenPointToRay(_mousePositionInput);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance)) {
            Vector3 mousePos = ray.GetPoint(distance);
            Vector3 direction = mousePos - playerManager.transform.position;
            direction.y = 0;
            _curDir = direction;
        }
    }


    private void UpdateCamZoom()
    {
        vCam.Lens.OrthographicSize = Mathf.Lerp(vCam.Lens.OrthographicSize, _targetSize, Time.deltaTime * 10f);
    }
    
    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
        _playerTarget = playerManager.transform.Find("Player_LookAt");

        // Null 체크 추가
        if (_playerTarget != null)
        {
            vCam.Follow = _playerTarget;
            vCam.LookAt = _playerTarget;
        }
        else
        {
            Debug.LogError("Player_LookAt Transform을 찾을 수 없습니다.");
        }
        
        TurnOnCamera();
        _enable = true;
        
        // -= 를 먼저 호출하여 중복 구독 방지
        WorldSceneChangeManager.OnSceneEndPhase -= ResetCamOcclusion;
        WorldSceneChangeManager.OnSceneEndPhase += ResetCamOcclusion;
    }

    public void SetOrthographicTargetSize(float value)
    {
        _targetSize = value;
    }

    public void SetMousePosition(Vector2 value)
    {
        _mousePositionInput = value;
    }

    public Vector3 GetPlayerOrthographicDir => _curOrthographicDirection.normalized;
    public Vector3 GetPlayerPerspectiveDir => _curDir.normalized;
    
    public Vector3 GetPlayerRightDir()
    {
        Vector3 upAxis = Vector3.up;
        Vector3 rightVector = Vector3.Cross(upAxis, _curOrthographicDirection.normalized);
        return rightVector.normalized;
    }

    public Vector3 GetCamForward()
    {
        return vCam.transform.forward;
    }

    public Vector3 GetCamRight()
    {
        return vCam.transform.right;
    }

    public void TurnOffCamera()
    {
        vCam.gameObject.SetActive(false);
    }

    public void TurnOnCamera()
    {
        vCam.gameObject.SetActive(true);
    }

    public void SetExplorationMode()
    {
        mainCamera.cullingMask = layerExploration;
    }

    public void SetBuildMode()
    {
        mainCamera.cullingMask = layerBuild;
    }

    private void ResetCamOcclusion()
    {
        _enable = false;
        // 씬 전환 시 모든 재질 복구
        foreach(var rd in _occludedRenderers.Keys)
        {
            if (rd != null && _occludedRenderers.TryGetValue(rd, out Material[] originalMats))
            {
                rd.materials = originalMats;
            }
        }
        _occludedRenderers.Clear();
    }
}