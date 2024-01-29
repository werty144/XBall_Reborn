using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public enum MessageType
{
    PlayerMovementAction = 1,
    Ready = 2,
    SendPing = 3,
    ReplyPing = 4,
    GameState = 5
}

public class P2PBase : MonoBehaviour
{
    protected Client Client;

    protected HSteamNetConnection Connection;
    
    protected virtual void Awake()
    {
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
    }

    public virtual void ConnectToServer(CSteamID serverID)
    {
        
    }

    public virtual void OnConnected(HSteamNetConnection connection)
    {
        Connection = connection;
        Client.OnConnected();
    }

    public virtual void SendReady()
    {
        
    }
    
    private void Update()
    {
        if (!SteamManager.Initialized) { return; }
        IntPtr[] messagePtrs = new IntPtr[1];
        int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(Connection, messagePtrs, 1);
         
        if (numMessages == 0)
        { 
            return;
        }
         
        for (int i = 0; i < numMessages; i++)
        {
            // Marshal the message from the unmanaged memory
            SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messagePtrs[i]);
            
            // Extract the message data
            byte[] managedArray = new byte[netMessage.m_cbSize];
            Marshal.Copy(netMessage.m_pData, managedArray, 0, netMessage.m_cbSize);
            SteamNetworkingMessage_t.Release(messagePtrs[i]);
            
            ProcessMessage(managedArray);
        }
    }

    protected virtual void ProcessMessage(byte[] message)
    {
        
    }

    private void OnDestroy()
    {
        SteamNetworkingSockets.CloseConnection(Connection, 0, "", false);
    }

    protected CSteamID GetPeerID()
    {
        SteamNetConnectionInfo_t connectionInfo;
        var connectionValid = SteamNetworkingSockets.GetConnectionInfo(Connection, out connectionInfo);
        if (!connectionValid ||
            connectionInfo.m_eState != ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected ||
            connectionInfo.m_identityRemote.IsInvalid())
        {
            Debug.LogWarning("Invalid connection");
            return new CSteamID();
        }
        return connectionInfo.m_identityRemote.GetSteamID();
    }
}
