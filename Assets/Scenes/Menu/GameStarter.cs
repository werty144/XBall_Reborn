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
    public CSteamID OpponentID;
    public CSteamID MyID;
    public bool IAmMaster;
}

public class GameStarter : MonoBehaviour
{
    public SetupInfo Info { get; protected set; }
    public bool IsTest { get; protected set; }

    public void Initiate(SetupInfo info)
    {
        Debug.Log("Initiate switching scenes" );
        Info = info;
        SceneManager.LoadScene("Game");
    }
}
