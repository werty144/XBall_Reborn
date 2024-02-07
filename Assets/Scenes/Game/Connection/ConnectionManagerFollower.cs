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
        CloseConnection();
        Debug.Log("Connecting to server");
        var identity = new SteamNetworkingIdentity();
        identity.SetSteamID(ServerID);
        
        var connectionParams = new SteamNetworkingConfigValue_t[2];
        connectionParams[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial;
        connectionParams[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        connectionParams[0].m_val = new SteamNetworkingConfigValue_t.OptionValue{ m_int32 = 5000 };
        connectionParams[1].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
        connectionParams[1].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        connectionParams[1].m_val = new SteamNetworkingConfigValue_t.OptionValue{ m_int32 = 1000 };
        
        Connection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, connectionParams.Length, connectionParams);
        
    }

    public override void OnConnected(HSteamNetConnection connection)
    {
        base.OnConnected(connection);
        GameManager.OnConnectedToServer();
    }

    public override void OnUnknownProblem()
    {
        CloseConnection();
        GameManager.OnConnectionUnknownProblem();
        ConnectToServer();
    }

    public override void OnRemoteProblem()
    {
        CloseConnection();
        ConnectToServer();
        GameManager.OnConnectionRemoteProblem();
    }

    public override void OnLocalProblem()
    {
        CloseConnection();
        ConnectToServer();
        GameManager.OnConnectionLocalProblem();
    }

    public override void OnClosedByPeer()
    {
        CloseConnection();
        ConnectToServer();
        GameManager.OnConnectionPeerDisconnected();
    }

    public void ReconnectTimeOut()
    {
        CloseConnection();
        ConnectToServer();
    }
}
