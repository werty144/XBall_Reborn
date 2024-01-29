using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
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

    // --------------------------- CLIENT METHODS -----------------------------
    public override void ConnectToServer(CSteamID serverID)
    {
        Client.OnConnected();
    }

    public override void SendReady()
    {
        Server.PeerReady(Steam.MySteamID());
    }

    public override void SendAction(IBufferMessage action)
    {
        Server.ProcessAction(Steam.MySteamID(), action);
    }

    // ----------------------------- SERVER METHODS --------------------------------
    public virtual void SendGameStart(CSteamID userID)
    {
        if (userID == Steam.MySteamID())
        {
            Client.OnGameStart();
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.GameStart);
            byte[] bytes = stream.ToArray();
            SendMessage(bytes);
        }
    }

    public virtual void SendGameState(CSteamID userID, GameState gameState)
    {
        if (userID == Steam.MySteamID())
        {
            Client.ReceiveState(gameState);
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.GameState);
            gameState.WriteTo(stream);
            byte[] bytes = stream.ToArray();
            SendMessage(bytes);
        }
    }

    protected override void ProcessMessage(byte[] message)
    {
        base.ProcessMessage(message);
        switch (message[0])
        {
            case (byte)MessageType.PlayerMovementAction:
                var action = ParseUtils.UnmarshalPlayerMovementAction(message);
                Server.ProcessAction(GetPeerID(), action);
                break;
            case (byte)MessageType.Ready:
                Server.PeerReady(GetPeerID());
                break;
        }
    }

    public virtual TimeSpan GetPingToUser(CSteamID userID)
    {
        if (userID == Steam.MySteamID())
        {
            return TimeSpan.Zero;
        }

        return Ping;
    }
}
