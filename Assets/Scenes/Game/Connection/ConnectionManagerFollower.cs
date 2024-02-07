using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public class ConnectionManagerFollower : ConnectionManagerBase
{
    private CSteamID ServerID;

    protected override void Start()
    {
        base.Start();
        
        ServerID = GameObject.FindWithTag("Global").GetComponent<GameStarter>().Info.OpponentID;
        ConnectToServer();
    }
    

    private void ConnectToServer()
    {
        Debug.Log("Connecting to server");
        var identity = new SteamNetworkingIdentity();
        identity.SetSteamID(ServerID);
        
        var connectionParams = new SteamNetworkingConfigValue_t[1];
        connectionParams[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial;
        connectionParams[0].m_val = new SteamNetworkingConfigValue_t.OptionValue{ m_int32 = 5000 };
        
        SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, null);
    }

    public override void OnConnected(HSteamNetConnection connection)
    {
        base.OnConnected(connection);
        GameManager.OnConnectedToServer();
    }

    public override void OnRemoteProblem()
    {
        ConnectToServer();
        GameManager.OnConnectionRemoteProblem();
    }

    public override void OnLocalProblem()
    {
        ConnectToServer();
        GameManager.OnConnectionLocalProblem();
    }

    public override void OnClosedByPeer()
    {
        ConnectToServer();
        GameManager.OnConnectionPeerDisconnected();
    }

    public void ReconnectTimeOut()
    {
        ConnectToServer();
    }
}
