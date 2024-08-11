using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCleanup : MonoBehaviour
{
    private void OnDestroy()
    {
        Time.timeScale = 1;
        Physics.autoSimulation = true;
        
        var gameStarter = GameObject.FindWithTag("Global").GetComponent<GameStarter>();
        Steam.LeaveLobby(gameStarter.Info.LobbyID.m_SteamID);
    }
}
