using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class Client : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject LoadingScreen;

    private P2PBase P2PManager;
    private CSteamID ServerID;
    
    private Dictionary<uint, PlayerController> Players = new ();

    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;
        
        CreatePlayers(setupInfo.NumberOfPlayers, setupInfo.IAmMaster);

        ServerID = setupInfo.IAmMaster ? Steam.MySteamID() : setupInfo.OpponentID;
        
        P2PManager = GameObject.FindWithTag("P2P").GetComponent<P2PBase>();
        
        LoadingScreen.SetActive(true);
    }

    private void Start()
    {
        P2PManager.ConnectToServer(ServerID);
    }
    
    void CreatePlayers(int n, bool IAmMaster)
    {
        var defaultPlaneWidth = 10;
        var defaultPlaneLength = 10;
        
        var floor = GameObject.FindWithTag("Floor");
        var scale = floor.GetComponent<Transform>().localScale;
        var fieldWidth = scale.x * defaultPlaneWidth;
        var fieldLength = scale.z * defaultPlaneLength;
        
        byte spareID = 0;
        float masterZ = -fieldLength / 4;
        float followerZ = fieldLength / 4;
        for (int i = 0; i < n; i++)
        {
            var x = fieldWidth * (i + 1) / (n + 1) - fieldWidth / 2;
            
            var masterPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, masterZ), Quaternion.identity);
            var controller = masterPlayer.GetComponent<PlayerController>();
            controller.Initialize(IAmMaster, spareID);
            Players[spareID] = controller;
            spareID++;
            
            var followerPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.identity);
            var followerContorller = followerPlayer.GetComponent<PlayerController>();
            followerContorller.Initialize(!IAmMaster, spareID);
            Players[spareID] = followerContorller;
            spareID++;
        }
    }
    
    public void OnConnected()
    {
        P2PManager.SendReady();
    }

    public void OnGameStart()
    {
        LoadingScreen.SetActive(false);
    }

    public void InputAction(IBufferMessage action)
    {
        
    }

    public void GameEnd()
    {
        
    }
}
