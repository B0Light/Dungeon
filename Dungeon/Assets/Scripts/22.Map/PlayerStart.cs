using System;
using UnityEngine;

public class PlayerStart : MonoBehaviour
{
    [Header("Spawn Settings")] 
    [SerializeField] private GameObject spawnVisual;
    private void Start()
    {
        if (spawnVisual != null)
            spawnVisual.SetActive(false);
        
        PlayerManager playerManager = GameManager.Instance.SpawnPlayer(gameObject.transform);
        playerManager.LoadGameDataFromCurrentCharacterDataSceneChange(ref WorldSaveGameManager.Instance.currentGameData);
        
        PlayerInputManager.Instance.SetControlActive(true);
        PlayerCameraController.Instance.LockOn(false);
        
        // 추후조정 : 모험시에만 활성화 
        WorldPlayerInventory.Instance.SetStartItemValue();
        
        // 추후 조정 : 복귀시에만 활성화
        WorldTimeManager.Instance.AdvanceTime();
        
        WorldTimeManager.Instance.ApplySkybox();
    }
}
