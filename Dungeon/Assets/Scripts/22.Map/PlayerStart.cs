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
        
        // 메뉴씬이 아닌경우 마우스 비활성화 
        PlayerInputManager.Instance.SetControlActive(!WorldSceneChangeManager.Instance.IsMenuScene());
        PlayerCameraController.Instance.LockOn(false);
        
        // 탈출맵에서 생성된 루팅가치를 활성화 
        if (WorldSceneChangeManager.Instance.IsExtractionMap())
        {
            WorldPlayerInventory.Instance.SetStartItemValue();
        }
        else if(WorldSceneChangeManager.Instance.IsShelter())
        {
            WorldTimeManager.Instance.AdvanceTime();
        }
        
        WorldTimeManager.Instance.ApplySkybox();
    }
}
