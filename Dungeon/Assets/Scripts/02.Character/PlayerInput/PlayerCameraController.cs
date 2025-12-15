using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : Singleton<PlayerCameraController>
{
    private bool _enable = false;
    [HideInInspector] public PlayerManager playerManager;
    [SerializeField] private Camera mainCamera;
    private readonly Vector3 _battleCamPosOffset = new Vector3(0, 2.5f, -3.3f);
    private readonly Vector3 _battleCamRotOffset = new Vector3(20, 0, 0);

    private Transform _playerTarget;
    
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera vCam; 

    [Header("Occlusion Settings")] 
    [SerializeField] private bool hideOption = true;
    [SerializeField] private LayerMask occlusionLayer; 
    [SerializeField] private float raycastDistanceOffset = 0.5f;

    [Header("Material Replacement")]
    [SerializeField] private Material replacementMaterial;

    [Header("Cam Mode Culling Mask")]
    [SerializeField] private LayerMask layerExploration;
    [SerializeField] private LayerMask layerBuild;
    
    // 딕셔너리에 원래 재질 정보 저장
    private readonly Dictionary<Renderer, Material[]> _occludedRenderers = new Dictionary<Renderer, Material[]>();
    
    // 최적화: Physics.RaycastNonAlloc()을 위한 배열 사전 할당
    private const int MAX_HITS = 10;
    private RaycastHit[] _raycastHits = new RaycastHit[MAX_HITS];
    
    // 방향 전환 
    private Vector2 _mousePositionInput = Vector2.zero;
    private Vector3 _curDirection;
    
    public void Update()
    {
        if(!_enable) return;
        HandleOcclusion();
        UpdateSight();
    }

    private void HandleOcclusion()
    {
        if(!hideOption) return;

        // 가려져야 할 오브젝트를 추적하기 위한 임시 HashSet
        var currentOccludedRenderers = new HashSet<Renderer>();

        Vector3 direction = (_playerTarget.position - mainCamera.transform.position).normalized;
        float distance = Vector3.Distance(mainCamera.transform.position, _playerTarget.position) - raycastDistanceOffset;
        
        int hitCount = Physics.RaycastNonAlloc(mainCamera.transform.position, direction, _raycastHits, distance, occlusionLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _raycastHits[i];
            
            if (hit.collider.CompareTag("Ignore_CamCollision")) 
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    currentOccludedRenderers.Add(renderer);

                    if (!_occludedRenderers.ContainsKey(renderer))
                    {
                        // 원본 재질을 저장하고 교체
                        _occludedRenderers[renderer] = renderer.sharedMaterials;
                        
                        Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                        for (int j = 0; j < newMaterials.Length; j++)
                        {
                            newMaterials[j] = replacementMaterial;
                        }
                        renderer.materials = newMaterials;
                    }
                }
            }
        }
        
        // 더 이상 가려지지 않는 오브젝트의 재질을 복원
        // _occludedRenderers 딕셔너리에서 현재 Raycast에 포함되지 않은 렌더러들을 찾습니다.
        var renderersToRestore = new List<Renderer>();
        foreach(var rd in _occludedRenderers.Keys)
        {
            if(!currentOccludedRenderers.Contains(rd))
            {
                renderersToRestore.Add(rd);
            }
        }

        foreach(var rd in renderersToRestore)
        {
            if (_occludedRenderers.TryGetValue(rd, out Material[] originalMats))
            {
                rd.materials = originalMats;
                _occludedRenderers.Remove(rd);
            }
        }
    }
    
    private void UpdateSight()
    {
        Ray ray = mainCamera.ScreenPointToRay(_mousePositionInput);
		
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); 
        if (groundPlane.Raycast(ray, out float distance)) {
			
            Vector3 mousePos = ray.GetPoint(distance);
            Vector3 direction = mousePos - playerManager.transform.position;
            direction.y = 0;
            _curDirection = direction;
            /*
            Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
            fieldOfViewSight.SetAimDirection(mousePos - playerPos);
            fieldOfViewSight.SetOrigin(playerPos + offset);
            */
        }
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

    public void SetMousePosition(Vector2 value)
    {
        _mousePositionInput = value;
    }

    public Vector3 GetPlayerDir => _curDirection.normalized;
    
    public Vector3 GetPlayerRightDir()
    {
        Vector3 upAxis = Vector3.up;
        Vector3 rightVector = Vector3.Cross(upAxis, _curDirection.normalized);
        return rightVector.normalized;
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