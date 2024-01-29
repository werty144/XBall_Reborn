// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Runtime.InteropServices;
// using System.Threading;
// using Google.Protobuf;
// using Steamworks;
// using UnityEngine;
// using UnityEngine.Assertions;
// using UnityEngine.Diagnostics;
//
// public enum MessageType
// {
//     PlayerMovementAction = 1,
//     Ready = 2,
//     SendPing = 3,
//     ReplyPing = 4,
//     GameState = 5
// }
//
// public class P2P : MonoBehaviour
// {
//     private GameManager GameManager;
//     private GameStarter GameStarter;
//
//     private HSteamNetConnection Connection;
//     private DateTime lastPingSent;
//     
//     // ---------------------------------- SETUP ------------------------------
//     private void OnEnable()
//     {
//         if (!SteamManager.Initialized) { return; }
//         GameStarter = GameObject.FindGameObjectWithTag("Global").GetComponent<GameStarter>();
//         SteamNetworkingUtils.InitRelayNetworkAccess();
//         SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
//     }
//
//     public void SetGameManager(GameManager gameManager)
//     {
//         GameManager = gameManager;
//     }
//
//     public void ConnectionEstablished(HSteamNetConnection connection)
//     {
//         CloseConnection();        
//         Connection = connection;
//         GameStarter.OnConnected();
//         SendPing();
//     }
//     
//     // ----------------------------------- CONNECTION ------------------------------
//     public void ConnectToPeer(CSteamID remoteID)
//     {
//         var identity = new SteamNetworkingIdentity();
//         identity.SetSteamID(remoteID);
//         SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, null);
//     }
//
//     public void CloseConnection()
//     {
//         SteamNetworkingSockets.CloseConnection(Connection, 0, "", false);
//     }
//     
//     // ------------------------------------- LISTENING ---------------------------------
//
//     private void Update()
//     {
//         if (!SteamManager.Initialized) { return; }
//         IntPtr[] messagePtrs = new IntPtr[1];
//         int numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(Connection, messagePtrs, 1);
//         
//         if (numMessages == 0)
//         {
//             return;
//         }
//         
//         for (int i = 0; i < numMessages; i++)
//         {
//             // Marshal the message from the unmanaged memory
//             SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messagePtrs[i]);
//         
//             // Extract the message data
//             byte[] managedArray = new byte[netMessage.m_cbSize];
//             Marshal.Copy(netMessage.m_pData, managedArray, 0, netMessage.m_cbSize);
//             
//             SteamNetworkingMessage_t.Release(messagePtrs[i]);
//             
//             ProcessMessage(managedArray);
//         }
//     }
//
//     public void ProcessMessage(byte[] message)
//     {
//         switch (message[0])
//         {
//             case (byte)MessageType.PlayerMovementAction:
//                 var action = ParseUtils.UnmarshalPlayerMovementAction(message);
//                 GameManager.OpponentAction(action);
//                 break;
//             case (byte)MessageType.Ready:
//                 GameStarter.PeerReady();
//                 break;
//             case (byte)MessageType.SendPing:
//                 ReplyPing();
//                 break;
//             case (byte)MessageType.ReplyPing:
//                 GameManager.LastRTT(DateTime.Now - lastPingSent);
//                 SendPing();
//                 break;
//             case (byte)MessageType.GameState:
//                 var gameState = ParseUtils.UnmarshalGameState(message);
//                 GameManager.ReceiveGameState(gameState);
//                 break;
//             default:
//                 Debug.LogWarning("Message of unknown type!");
//                 break;
//         }
//     }
//     
//     // -------------------------------- PING ------------------------------------
//     void SendPing()
//     {
//         lastPingSent = DateTime.Now;
//         using (MemoryStream stream = new MemoryStream())
//         {
//             stream.WriteByte((byte)MessageType.SendPing);
//             byte[] bytes = stream.ToArray();
//             SendMessage(bytes);
//         }
//     }
//
//     void ReplyPing()
//     {
//         using (MemoryStream stream = new MemoryStream())
//         {
//             stream.WriteByte((byte)MessageType.ReplyPing);
//             byte[] bytes = stream.ToArray();
//             SendMessage(bytes);
//         }
//     }
//
//     // -------------------------------- Senders ---------------------------------
//     
//     public void SendAction(CSteamID peerID, IBufferMessage action)
//     {
         // using (MemoryStream stream = new MemoryStream())
         // {
         //     switch (action)
         //     {
         //         case PlayerMovementAction:
         //             stream.WriteByte((byte)MessageType.PlayerMovementAction);
         //             break;
         //         default:
         //             Debug.LogWarning("Unknown action");
         //             break;
         //     }
         //     
         //     action.WriteTo(stream);
         //     byte[] bytes = stream.ToArray();
         //     SendMessageToPeer(peerID, bytes);
         // }
//     }
//
//     public void SendReadyMessage(CSteamID peerID)
//     {
//         Debug.Log("Sending ready message");
//         using (MemoryStream stream = new MemoryStream())
//         {
//             stream.WriteByte((byte)MessageType.Ready);
//             byte[] bytes = stream.ToArray();
//             SendMessageToPeer(peerID, bytes);
//         }
//     }
//
//     public void SendGameStateMessage(CSteamID peerID, GameState gameState)
//     {
//         using (MemoryStream stream = new MemoryStream())
//         {
//             stream.WriteByte((byte)MessageType.GameState);
//             gameState.WriteTo(stream);
//             byte[] bytes = stream.ToArray();
//             SendMessageToPeer(peerID, bytes);
//         }
//     }
//     
//     void SendMessageToPeer(CSteamID remoteID, byte[] message)
//     {
//         if (!SteamManager.Initialized) { return; }
         // SteamNetConnectionInfo_t connectionInfo;
         // var connectionValid = SteamNetworkingSockets.GetConnectionInfo(Connection, out connectionInfo);
         // if (!connectionValid || 
         //     connectionInfo.m_eState != ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected ||
         //     connectionInfo.m_identityRemote.IsInvalid() ||
         //     connectionInfo.m_identityRemote.GetSteamID() != remoteID)
         // {
//             Debug.LogWarning("No connection with " + remoteID);
//             return;
//         }
//         IntPtr unmanagedPointer = Marshal.AllocHGlobal(message.Length);
//         Marshal.Copy(message, 0, unmanagedPointer, message.Length);
//         SteamNetworkingSockets.SendMessageToConnection(Connection, unmanagedPointer, (uint)message.Length, 8, out _);
//         Marshal.FreeHGlobal(unmanagedPointer);
//     }
//
//     void SendMessage(byte[] message)
//     {
//         SteamNetConnectionInfo_t connectionInfo;
//         var connectionValid = SteamNetworkingSockets.GetConnectionInfo(Connection, out connectionInfo);
//         Assert.IsTrue(connectionValid);
//         Assert.AreEqual(ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected, connectionInfo.m_eState);
//         SendMessageToPeer(connectionInfo.m_identityRemote.GetSteamID(), message);
//     }
// }
