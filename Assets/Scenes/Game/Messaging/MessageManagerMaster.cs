using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class MessageManagerMaster : MonoBehaviour, MessageManager
{
    private ConnectionManager ConnectionManager;
    protected Client Client;
    protected GameManager GameManager;
    private PingManager PingManager;
    protected Server Server;
    private void Start()
    {
        ConnectionManager = GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>();
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
        Server = GameObject.FindWithTag("Server").GetComponent<Server>();
    }

    // --------------------------- CLIENT METHODS -----------------------------
    public void SendReady()
    {
        if (Server == null)
        {
            Server = GameObject.FindWithTag("Server").GetComponent<Server>();
        }
        Server.PeerReady(Steam.MySteamID());
    }

    public void SendAction(IBufferMessage action)
    {
        Server.ProcessAction(Steam.MySteamID(), action);
    }

    // ----------------------------- PING --------------------------------
    public virtual void SendPing()
    {
        using MemoryStream stream = new MemoryStream();
        stream.WriteByte((byte)MessageType.Ping);
        byte[] bytes = stream.ToArray();
        ConnectionManager.SendMessage(bytes);
    }

    public void SendReplyPing()
    {
        using MemoryStream stream = new MemoryStream();
        stream.WriteByte((byte)MessageType.ReplyPing);
        byte[] bytes = stream.ToArray();
        ConnectionManager.SendMessage(bytes);
    }
    
    // ----------------------------- SERVER METHODS --------------------------------
    public virtual void SendGameStart(CSteamID userID)
    {
        if (userID == Steam.MySteamID())
        {
            GameManager.OnGameStart();
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.GameStart);
            byte[] bytes = stream.ToArray();
            ConnectionManager.SendMessage(bytes);
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
            ConnectionManager.SendMessage(bytes);
        }
    }
    
    public virtual void SendActionResponse(CSteamID userID, ActionResponse actionResponse)
    {
        if (userID == Steam.MySteamID())
        {
            Client.ReceiveActionResponse(actionResponse);
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.ActionResponse);
            actionResponse.WriteTo(stream);
            byte[] bytes = stream.ToArray();
            ConnectionManager.SendMessage(bytes);
        }
    }

    // ----------------------------- PROCESS MESSAGES --------------------------------
    public void ProcessMessage(byte[] message)
    {
        switch (message[0])
        {
            case (byte)MessageType.Ping:
                PingManager.ReceivePing();
                break;
            case (byte)MessageType.ReplyPing:
                PingManager.ReceiveReplyPing();
                break;
            case (byte)MessageType.PlayerMovementAction:
                var action = ParseUtils.UnmarshalPlayerMovementAction(message);
                Server.ProcessAction(ConnectionManager.GetPeerID(), action);
                break;
            case (byte)MessageType.Ready:
                Server.PeerReady(ConnectionManager.GetPeerID());
                break;
        }
    }
}
