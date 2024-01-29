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
    public bool IAmMaster;
}

public class GameStarter : MonoBehaviour
{
    public SetupInfo Info { get; private set; }
    public bool IsTest { get; private set; }

    public void Initiate(SetupInfo info)
    {
        Debug.Log("Initiate switching scenes" );
        Info = info;
        SceneManager.LoadScene("Game");
    }
    
    public void TestStartLoad()
    {
        IsTest = true;

        Info = new SetupInfo
        {
            IAmMaster = true,
            NumberOfPlayers = 3,
            OpponentID = new CSteamID(1)
        };
        SceneManager.LoadScene("Game");
    }
}
