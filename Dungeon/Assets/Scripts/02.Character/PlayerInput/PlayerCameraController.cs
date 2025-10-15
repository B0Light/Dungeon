using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : Singleton<PlayerCameraController>
{
    private bool _enableOcclusion = false;
    [HideInInspector] public PlayerManager playerManager;
    public Camera mainCamera;

    private Transform _playerTarget;
    
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera vCam; 
    [SerializeField] private CinemachineInputAxisController cameraController;

    [Header("Occlusion Settings")] 
    [SerializeField] private bool hideOption = true;
    [SerializeField] private LayerMask occlusionLayer; 
    [SerializeField] private float raycastDistanceOffset = 0.5f;

    [Header("Material Replacement")]
    [SerializeField] private Material replacementMaterial;
    
    // 딕셔너리에 원래 재질 정보 저장
    private readonly Dictionary<Renderer, Material[]> _occludedRenderers = new Dictionary<Renderer, Material[]>();
    
    // 최적화: Physics.RaycastNonAlloc()을 위한 배열 사전 할당
    private const int MAX_HITS = 10;
    private RaycastHit[] _raycastHits = new RaycastHit[MAX_HITS];
    
    public void Update()
    {
        if(!_enableOcclusion) return;
        HandleOcclusion();
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
        foreach(var renderer in _occludedRenderers.Keys)
        {
            if(!currentOccludedRenderers.Contains(renderer))
            {
                renderersToRestore.Add(renderer);
            }
        }

        foreach(var renderer in renderersToRestore)
        {
            if (_occludedRenderers.TryGetValue(renderer, out Material[] originalMats))
            {
                renderer.materials = originalMats;
                _occludedRenderers.Remove(renderer);
            }
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
        _enableOcclusion = true;
        
        // -= 를 먼저 호출하여 중복 구독 방지
        WorldSceneChangeManager.OnSceneEndPhase -= ResetCamOcclusion;
        WorldSceneChangeManager.OnSceneEndPhase += ResetCamOcclusion;
    }
    
    public Vector3 GetCameraPosition()
    {
        return mainCamera.transform.position;
    }

    public Vector3 GetCameraForward()
    {
        return mainCamera.transform.forward;
    }

    public Vector3 GetCameraForwardZeroedYNormalized()
    {
        Vector3 forward = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
        return forward.normalized;
    }
    
    public Vector3 GetCameraRightZeroedYNormalized()
    {
        Vector3 right = new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
        return right.normalized;
    }

    public float GetCameraTiltX()
    {
        return mainCamera.transform.eulerAngles.x;
    }

    public void TurnOffCamera()
    {
        vCam.gameObject.SetActive(false);
    }

    public void TurnOnCamera()
    {
        vCam.gameObject.SetActive(true);
    }

    public void SetCameraControllerEnable(bool newValue)
    {
        if (cameraController != null && cameraController.enabled != newValue)
            cameraController.enabled = newValue;
    }

    private void ResetCamOcclusion()
    {
        _enableOcclusion = false;
        // 씬 전환 시 모든 재질 복구
        foreach(var renderer in _occludedRenderers.Keys)
        {
            if (renderer != null && _occludedRenderers.TryGetValue(renderer, out Material[] originalMats))
            {
                renderer.materials = originalMats;
            }
        }
        _occludedRenderers.Clear();
    }
}