using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Steamworks;
using UnityEngine;

public class P2P : MonoBehaviour
{

    private HSteamNetConnection Connection;
    private string awaitingMessage = ""; 
    private void OnEnable()
    {
        SteamNetworkingUtils.InitRelayNetworkAccess();
        SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
    }

    public void ConnectionEstablished(HSteamNetConnection connection)
    {
        Connection = connection;
        if (awaitingMessage != "")
        {
            SendString(awaitingMessage);
            awaitingMessage = "";
        }
    }
    
    public void SendMessageToPeer(CSteamID remoteID, string message)
    {
        SteamNetConnectionInfo_t connectionInfo;
        var connectionValid = SteamNetworkingSockets.GetConnectionInfo(Connection, out connectionInfo);
        if (!connectionValid || 
            connectionInfo.m_eState != ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected ||
            connectionInfo.m_identityRemote.IsInvalid() ||
            connectionInfo.m_identityRemote.GetSteamID() != remoteID)
        {
            Debug.Log("No connection with " + remoteID + ". Establishing...");
            var identity = new SteamNetworkingIdentity();
            identity.SetSteamID(remoteID);
            SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, null);
            awaitingMessage = message;
            return;
        }
        SendString(message);
    }

    private void SendString(string msg)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);

        // Allocate unmanaged memory and copy the byte array to it
        IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, unmanagedPointer, data.Length);

        // Send the message
        SteamNetworkingSockets.SendMessageToConnection(Connection, unmanagedPointer, (uint)data.Length, 0, out _);
        
        
        // Free the unmanaged memory
        Marshal.FreeHGlobal(unmanagedPointer);
    }

    private void Update()
    {
        IntPtr[] messagePtrs = new IntPtr[1];
        int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(Connection, messagePtrs, 1);
        
        if (numMessages == 0)
        {
            // No messages to process
            return;
        }
        
        for (int i = 0; i < numMessages; i++)
        {
            // Marshal the message from the unmanaged memory
            SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messagePtrs[i]);
        
            // Extract the message data
            byte[] managedArray = new byte[netMessage.m_cbSize];
            Marshal.Copy(netMessage.m_pData, managedArray, 0, netMessage.m_cbSize);
        
            // Convert the byte array to a string (assuming UTF-8 encoding)
            string receivedMessage = System.Text.Encoding.UTF8.GetString(managedArray);
            Debug.Log("Received message: " + receivedMessage);
        
            // Release the message
            SteamNetworkingMessage_t.Release(messagePtrs[i]);
        }
    }
}
