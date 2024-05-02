using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct SetupInfo
{
    public int NumberOfPlayers;
    public string Speed;
    public CSteamID OpponentID;
    public CSteamID MyID;
    public bool IAmMaster;
}

public class GameStarter : MonoBehaviour
{
    public SetupInfo Info { get; protected set; }
    public bool IsTest { get; set; }

    public void Initiate(SetupInfo info)
    {
        Debug.Log("Initiate switching scenes" );
        Info = info;
        GameObject.FindWithTag("SceneTransition").GetComponent<SceneTransition>().LoadScene("Game");
        switch (Info.Speed)
        {
            case LobbyManager.SpeedSlow:
                Time.timeScale = 0.5f;
                break;
            case LobbyManager.SpeedNormal:
                Time.timeScale = 1f;
                break;
            case LobbyManager.SpeedFast:
                Time.timeScale = 2f;
                break;
            default:
                Debug.LogWarning("Unknown speed");
                break;
        }
    }
}
