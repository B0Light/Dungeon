using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGameSessionManager : Singleton<WorldGameSessionManager>
{
    [Header("Active Players In Session")]
    public List<PlayerManager> players = new List<PlayerManager>();

    public void AddPlayerToActivePlayersList(PlayerManager player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }   

        for(int i = players.Count -1; i > -1; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }

    public void RemovePlayerToActivePlayersList(PlayerManager player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
        }

        for (int i = players.Count - 1; i > -1; i--)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
            }
        }
    }
}
