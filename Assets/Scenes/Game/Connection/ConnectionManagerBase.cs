using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public abstract class ConnectionManagerBase : MonoBehaviour, ConnectionManager
{
    protected GameManager GameManager;
    
    protected HSteamNetConnection Connection;
    private MessageManager MessageManager;
    private PingManager PingManager;

    private Callback<SteamNetConnectionStatusChangedCallback_t> m_StateConnectionStatusChangeCallback;
    
    
    protected virtual void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        
        m_StateConnectionStatusChangeCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionChanged);
    }

    private void OnConnectionChanged(SteamNetConnectionStatusChangedCallback_t pCallback)
    {
        switch (pCallback.m_info.m_eState)
        {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                SteamNetworkingSockets.AcceptConnection(pCallback.m_hConn);
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                Debug.Log(
                    "Connection with " + pCallback.m_info.m_identityRemote.GetSteamID().m_SteamID + " established");
                OnConnected(pCallback.m_hConn);
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                Debug.LogWarning("Problem detected locally: " + pCallback.m_info.m_eEndReason + ". " +
                                 pCallback.m_info.m_szEndDebug);
                switch (pCallback.m_info.m_eEndReason)
                {
                    case (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_Timeout:
                        OnRemoteProblem();
                        break;
                    case (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_Timeout:
                        if (pCallback.m_eOldState ==
                            ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
                        {
                            ((ConnectionManagerFollower)this).ReconnectTimeOut();
                        }
                        else
                        {
                            OnLocalProblem();
                        }

                        break;
                    default:
                        OnUnknownProblem();
                        Debug.LogWarning("Unknown problem detected locally");
                        break;
                }
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                Debug.Log("Closed by peer");
                switch (pCallback.m_eOldState)
                {
                    case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                        OnClosedByPeerWhileConnecting();
                        break;
                    case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                        OnClosedByPeerWhenActive();
                        break;
                    default:
                        Debug.LogWarning("Unsupported connection close");
                        break;
                }

                break;
        }
    }

    public virtual void OnConnected(HSteamNetConnection connection)
    {
        Connection = connection;
        PingManager.OnConnected();
    }

    public abstract void OnUnknownProblem();
    public abstract void OnClosedByPeerWhenActive();

    public abstract void OnClosedByPeerWhileConnecting();

    public abstract void OnRemoteProblem();

    public abstract void OnLocalProblem();
    

    public void SendMessage(byte[] message)
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
             
            MessageManager.ProcessMessage(managedArray);
        }
    }
    
    public CSteamID GetPeerID()
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

    public void CloseConnection()
    {
        SteamNetworkingSockets.CloseConnection(Connection, 0, "", true);
    }

    public void OnDestroy()
    {
        CloseConnection();
        m_StateConnectionStatusChangeCallback.Dispose();
    }
}
