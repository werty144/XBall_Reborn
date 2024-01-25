using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    private P2P P2PManager;
    private SetupInfo Info;
    private ulong LobbyID;

    private bool IAmReady;
    private bool OpponentReady;
    private bool ConnectionReady;

    private void OnEnable()
    {
        P2PManager = GameObject.FindWithTag("Global").GetComponent<P2P>();
    }

    public void Initiate(SetupInfo info, ulong lobbyID)
    {
        Debug.Log("Initiate switching scenes" );
        Info = info;
        LobbyID = lobbyID;
        SceneManager.LoadScene("Game");
        
        if (info.IAmMaster)
        {
            P2PManager.ConnectToPeer(info.OpponentID);
        }
    }

    public SetupInfo GetSetupInfo()
    {
        return Info;
    }

    public void OnConnected()
    {
        Debug.Log("Connected to peer");
        ConnectionReady = true;
        if (IAmReady)
        {
            SendReadyMessage();
        }
    }

    public void GameManagerReady(GameManager gameManager)
    {
        Debug.Log("Game manager ready");
        IAmReady = true;
        P2PManager.SetGameManager(gameManager);
        SendReadyMessage();
        CheckAllReady();
    }

    private void SendReadyMessage()
    {
        if (!ConnectionReady) { return; }
        P2PManager.SendReadyMessage(Info.OpponentID);
    }

    public void PeerReady()
    {
        Debug.Log("Peer ready");
        OpponentReady = true;
        CheckAllReady();
    }

    private void CheckAllReady()
    {
        if (IAmReady && OpponentReady)
        {
            Debug.Log("Everyone ready!");
            LeaveLobby();
        }
    }

    private void LeaveLobby()
    {
        Debug.Log("Leaving lobby");
        Clear();
        Steam.LeaveLobby(LobbyID);
    }

    private void Clear()
    {
        IAmReady = false;
        OpponentReady = false;
        ConnectionReady = false;
    }
}
