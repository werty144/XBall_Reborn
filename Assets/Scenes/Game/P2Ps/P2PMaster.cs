using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class P2PMaster : P2PBase
{
    protected Server Server;

    protected override void Awake()
    {
        base.Awake();
        Server = GameObject.FindWithTag("Server").GetComponent<Server>();
    }

    public override void ConnectToServer(CSteamID serverID)
    {
        Client.OnConnected();
    }

    public override void SendReady()
    {
        Server.PeerReady(Steam.MySteamID());
    }

    // Server method
    public void SendGameStart(CSteamID userID)
    {
        if (userID == Steam.MySteamID())
        {
            Client.OnGameStart();
        }
        else
        {
            // TODO: send message to follower
        }
    }

    protected override void ProcessMessage(byte[] message)
    {
        switch (message[0])
        {
            case (byte)MessageType.PlayerMovementAction:
                var action = ParseUtils.UnmarshalPlayerMovementAction(message);
                Server.ProcessAction(GetPeerID(), action);
                break;
            case (byte)MessageType.Ready:
                Server.PeerReady(GetPeerID());
                break;
            case (byte)MessageType.SendPing:
                // ReplyPing();
                break;
            case (byte)MessageType.ReplyPing:
                // GameManager.LastRTT(DateTime.Now - lastPingSent);
                // SendPing();
                break;
            default:
                Debug.LogWarning("Message of unknown type for P2PMaster: " + message[0]);
                break;
        }
    }    
}
