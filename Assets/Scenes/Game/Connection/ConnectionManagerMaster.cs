using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public class ConnectionManagerMaster : ConnectionManagerBase
{
    private Server Server;
    protected override void Start()
    {
        base.Start();

        Server = GameObject.FindWithTag("Server").GetComponent<Server>();
        
        GameManager.OnConnectedToServer();
    }

    public void OnConnecting(HSteamNetConnection connection)
    {
        SteamNetConnectionInfo_t connectionInfo;
        SteamNetworkingSockets.GetConnectionInfo(connection, out connectionInfo);
        if (connectionInfo.m_identityRemote.GetSteamID() ==
            GameObject.FindWithTag("Global").GetComponent<GameStarter>().Info.OpponentID)
        {
            SteamNetworkingSockets.AcceptConnection(connection);
        }
        else
        {
            Debug.LogWarning("Rejecting connection not an opponent");
            SteamNetworkingSockets.CloseConnection(connection, 2, "Not an opponent", false);
        }
    }

    public override void OnConnected(HSteamNetConnection connection)
    {
        base.OnConnected(connection);
        
        Server.PeerConnected();
    }

    public override void OnUnknownProblem()
    {
        CloseConnection();
        Server.PeerDropped();
        GameManager.OnConnectionUnknownProblem();
    }

    public override void OnClosedByPeerWhenActive()
    {
        CloseConnection();
        Server.PeerDropped();
        GameManager.OnConnectionPeerDisconnected();
    }

    public override void OnClosedByPeerWhileConnecting()
    {
        CloseConnection();
    }

    public override void OnRemoteProblem()
    {
        CloseConnection();
        Server.PeerDropped();
        GameManager.OnConnectionRemoteProblem();
    }

    public override void OnLocalProblem()
    {
        CloseConnection();
        Server.PeerDropped();
        GameManager.OnConnectionLocalProblem();
    }
}
