using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;
using Steamworks;
using UnityEngine;
using UnityEngine.Assertions;

public enum MessageType
{
    PlayerMovementAction = 1,
    Ready = 2,
    SendPing = 3,
    ReplyPing = 4,
    GameState = 5,
    GameStart = 6
}

public class P2PBase : MonoBehaviour
{
    protected Client Client;

    protected HSteamNetConnection Connection;
    private DateTime lastPingSent;
    protected TimeSpan Ping;
    
    protected virtual void Awake()
    {
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
    }

    public virtual void ConnectToServer(CSteamID serverID)
    {
        
    }

    public void OnConnected(HSteamNetConnection connection)
    {
        Connection = connection;
        Client.OnConnected();
        SendPing();
    }

    public virtual void SendReady()
    {
        
    }

    public virtual void SendAction(IBufferMessage action)
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
        switch (message[0])
        {
            case (byte)MessageType.SendPing:
                ReplyPing();
                break;
            case (byte)MessageType.ReplyPing:
                Ping = (DateTime.Now - lastPingSent) / 2;
                SendPing();
                break;
        }
    }

    private void OnDestroy()
    {
        SteamNetworkingSockets.CloseConnection(Connection, 0, "", false);
    }
    
    protected void SendMessage(byte[] message)
    {
        SteamNetConnectionInfo_t connectionInfo;
        var connectionValid = SteamNetworkingSockets.GetConnectionInfo(Connection, out connectionInfo);
        if (!connectionValid ||
            ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected != connectionInfo.m_eState)
        {
            Debug.LogWarning("No connection to send message");
            return;
        }
        IntPtr unmanagedPointer = Marshal.AllocHGlobal(message.Length);
        Marshal.Copy(message, 0, unmanagedPointer, message.Length);
        SteamNetworkingSockets.SendMessageToConnection(Connection, unmanagedPointer, (uint)message.Length, 8, out _);
        Marshal.FreeHGlobal(unmanagedPointer);
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
    
    void SendPing()
     {
         lastPingSent = DateTime.Now;
         using MemoryStream stream = new MemoryStream();
         stream.WriteByte((byte)MessageType.SendPing);
         byte[] bytes = stream.ToArray();
         SendMessage(bytes);
     }

     void ReplyPing()
     {
         using MemoryStream stream = new MemoryStream();
         stream.WriteByte((byte)MessageType.ReplyPing);
         byte[] bytes = stream.ToArray();
         SendMessage(bytes);
     }

     public virtual TimeSpan GetPing()
     {
         return Ping;
     }
}
