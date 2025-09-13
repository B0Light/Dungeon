using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject playerPrefab;

    private PlayerManager _playerManager;
    
    public PlayerManager SpawnPlayer(Transform spawnPoint = null)
    {
        GameObject spawnedPlayer = spawnPoint == null
            ? Instantiate(playerPrefab)
            : Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        _playerManager = spawnedPlayer.GetComponent<PlayerManager>();
        return _playerManager;
    }

    public PlayerManager GetPlayer()
    {
        if(_playerManager == null) 
            Debug.LogError("No Player In This Game");
        return _playerManager;
    }

    public void HandlePostDeath_Continue()
    {
        if (WorldSceneChangeManager.Instance.GetCurSceneIndex() == 2) // tutorial
        {
            string curPlayerName = WorldSaveGameManager.Instance.currentGameData.characterName;
            WorldSaveGameManager.Instance.DeleteCurrentGame();
            WorldSaveGameManager.Instance.AttemptToCreateNewGame(curPlayerName);
        }
        else
        {
            _playerManager.playerItemConsumeManager.UseItem(300);
            WorldSceneChangeManager.Instance.LoadShelter(); // shelter
        }
    }

    public void HandlePostDeath_BackToTitle()
    {
        if (WorldSceneChangeManager.Instance.GetCurSceneIndex() == 2) // tutorial
        {
            WorldSaveGameManager.Instance.DeleteCurrentGame();
        }
        
        WorldSceneChangeManager.Instance.LoadSceneAsync(1);
    }
    
}