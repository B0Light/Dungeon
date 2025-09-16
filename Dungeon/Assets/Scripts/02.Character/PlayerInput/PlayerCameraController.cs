using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Serialization;

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
    
    private readonly List<Renderer> _hiddenRenderers = new List<Renderer>();
    
    public void Update()
    {
        if(!_enable) return;
        HandleOcclusion();
    }

    private void HandleOcclusion()
    {
        if(!hideOption) return;
        // First, restore any previously hidden objects
        foreach (var rendererObj in _hiddenRenderers)
        {
            if (rendererObj != null) // Check if the object still exists
            {
                rendererObj.enabled = true;
            }
        }
        _hiddenRenderers.Clear();

        // Perform a raycast from the camera to the player
        Vector3 direction = (_playerTarget.position - mainCamera.transform.position).normalized;
        float distance = Vector3.Distance(mainCamera.transform.position, _playerTarget.position) - raycastDistanceOffset;
        RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position, direction, distance, occlusionLayer);

        foreach (var hit in hits)
        {
            // Find the renderer on the hit object and disable it
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && !_hiddenRenderers.Contains(renderer))
            {
                renderer.enabled = false;
                _hiddenRenderers.Add(renderer);
            }
        }
    }
    
    public void SetPlayer(PlayerManager player)
    {
        playerManager = player;
        _playerTarget = playerManager.transform.Find("Player_LookAt");
        _originTarget = playerManager.transform.Find("TargetLockOnPos");
        _lockOnTarget = _originTarget;

        // Cinemachine 카메라 설정
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
