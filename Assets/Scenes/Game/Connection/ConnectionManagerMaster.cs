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
        GameManager.OnPeerConnected();
    }

    public override void OnRemoteProblem()
    {
        Server.PeerDropped();
        GameManager.OnConnectionRemoteProblem();
    }

    public override void OnLocalProblem()
    {
        Server.PeerDropped();
        GameManager.OnConnectionLocalProblem();
    }

    public override void OnClosedByPeer()
    {
        GameManager.OnConnectionPeerDisconnected();
    }
}
