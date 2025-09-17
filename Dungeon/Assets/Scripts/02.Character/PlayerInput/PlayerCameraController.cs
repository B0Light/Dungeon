using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraController : Singleton<PlayerCameraController>
{
    private bool _enable = false;
    [HideInInspector] public PlayerManager playerManager;
    public Camera mainCamera;

    private Transform _playerTarget;
    private Transform _lockOnTarget;
    private Transform _originTarget;
    private float _rotationX;
    private float _rotationY;

    private Transform _cameraTransform;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera vCam; 
    [SerializeField] private CinemachineInputAxisController cameraController;

    [Header("Occlusion Settings")] 
    [SerializeField] private bool hideOption = true;
    [SerializeField] private LayerMask occlusionLayer; 
    [SerializeField] private float raycastDistanceOffset = 0.5f;

    // Added: Material for replacement and a dictionary to store original materials
    [Header("Material Replacement")]
    [SerializeField] private Material replacementMaterial;
    
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
    
    // 최적화: Physics.RaycastNonAlloc()을 위한 배열 사전 할당
    private const int MAX_HITS = 10;
    private RaycastHit[] _raycastHits = new RaycastHit[MAX_HITS];

    public void Update()
    {
        if(!_enable) return;
        HandleOcclusion();
    }

    private void HandleOcclusion()
    {
        if(!hideOption) return;

        // Restore materials of objects that are no longer occluded
        var renderersToRestore = new List<Renderer>(_originalMaterials.Keys);

        // Perform a raycast from the camera to the player
        Vector3 direction = (_playerTarget.position - mainCamera.transform.position).normalized;
        float distance = Vector3.Distance(mainCamera.transform.position, _playerTarget.position) - raycastDistanceOffset;
        
        // 최적화: RaycastNonAlloc() 사용 및 occlusionLayer 적용
        int hitCount = Physics.RaycastNonAlloc(mainCamera.transform.position, direction, _raycastHits, distance, occlusionLayer);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _raycastHits[i];

            // Check the tag of the hit object and ensure it has a Renderer
            if (hit.collider.CompareTag("Ignore_CamCollision")) 
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // If the object is currently occluded, remove it from the restore list
                    if (renderersToRestore.Contains(renderer))
                    {
                        renderersToRestore.Remove(renderer);
                    }

                    // Store original materials if not already stored
                    if (!_originalMaterials.ContainsKey(renderer))
                    {
                        _originalMaterials[renderer] = renderer.materials;
                    }

                    // Replace all materials with the replacement material
                    Material[] newMaterials = new Material[renderer.materials.Length];
                    for (int j = 0; j < newMaterials.Length; j++)
                    {
                        newMaterials[j] = replacementMaterial;
                    }
                    renderer.materials = newMaterials;
                }
            }
        }

        // Restore materials for objects that are no longer being hit
        foreach (var renderer in renderersToRestore)
        {
            if (_originalMaterials.TryGetValue(renderer, out Material[] originalMats))
            {
                renderer.materials = originalMats;
                _originalMaterials.Remove(renderer);
            }
        }
    }
    
    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
        _playerTarget = playerManager.transform.Find("Player_LookAt");
        _originTarget = playerManager.transform.Find("TargetLockOnPos");
        _lockOnTarget = _originTarget;

        vCam.Follow = _playerTarget;
        vCam.LookAt = _playerTarget;
        
        TurnOnCamera();
        _enable = true;
    }
    
    public void LockOn(bool enable, Transform newLockOnTarget = null)
    {
        var orbitalFollow = vCam.GetComponent<CinemachineOrbitalFollow>();
        orbitalFollow.RecenteringTarget = CinemachineOrbitalFollow.ReferenceFrames.TrackingTarget;
        
        if(playerManager.playerVariableManager.CLVM.isStopped)
            StartCoroutine(ResetCamCoroutine());
        else if (enable)
        {
            orbitalFollow.HorizontalAxis.Recentering.Wait = 0f;
            orbitalFollow.HorizontalAxis.Recentering.Time = 0.5f;
            orbitalFollow.HorizontalAxis.Recentering.Enabled = true;
            orbitalFollow.VerticalAxis.Recentering.Wait = 0f;
            orbitalFollow.VerticalAxis.Recentering.Time = 0.5f;
            orbitalFollow.VerticalAxis.Recentering.Enabled = true;
            cameraController.enabled = false;
        }
        else
        {
            orbitalFollow.HorizontalAxis.Recentering.Enabled = false;
            orbitalFollow.VerticalAxis.Recentering.Enabled = false;
            cameraController.enabled = true;
        }
            
        _lockOnTarget = newLockOnTarget != null ? newLockOnTarget : _originTarget;
    }

    private IEnumerator ResetCamCoroutine()
    {
        var orbitalFollow = vCam.GetComponent<CinemachineOrbitalFollow>();
        orbitalFollow.RecenteringTarget = CinemachineOrbitalFollow.ReferenceFrames.TrackingTarget;
        
        orbitalFollow.HorizontalAxis.Recentering.Wait = 0f;
        orbitalFollow.HorizontalAxis.Recentering.Time = 0.5f;
        orbitalFollow.HorizontalAxis.Recentering.Enabled = true;
        orbitalFollow.VerticalAxis.Recentering.Wait = 0f;
        orbitalFollow.VerticalAxis.Recentering.Time = 0.5f;
        orbitalFollow.VerticalAxis.Recentering.Enabled = true;
        cameraController.enabled = false;
        yield return new WaitForSeconds(1f);
        orbitalFollow.HorizontalAxis.Recentering.Enabled = false;
        orbitalFollow.VerticalAxis.Recentering.Enabled = false;
        cameraController.enabled = true;
    }
    
    public Vector3 GetCameraPosition()
    {
        return mainCamera.transform.position;
    }

    public Vector3 GetCameraForward()
    {
        return mainCamera.transform.forward;
    }

    private Vector3 GetCameraForwardZeroedY()
    {
        return new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
    }

    public Vector3 GetCameraForwardZeroedYNormalized()
    {
        return GetCameraForwardZeroedY().normalized;
    }
    
    private Vector3 GetCameraRightZeroedY()
    {
        return new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
    }

    public Vector3 GetCameraRightZeroedYNormalized()
    {
        return GetCameraRightZeroedY().normalized;
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
        var currentValue = cameraController.enabled;
        if (currentValue != newValue)
            cameraController.enabled = newValue;
    }
}