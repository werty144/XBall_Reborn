using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Steamworks;
using UnityEngine;
using UnityEngine.Profiling;

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

    public virtual void SendAction(IBufferMessage action)
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

    public void SendResumeGame(CSteamID userID, GameState gameState)
    {
        if (userID == Steam.MySteamID())
        {
            Client.ReceiveResumeGame(gameState);
            GameManager.ResumeGame();
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.ResumeGame);
            gameState.WriteTo(stream);
            byte[] bytes = stream.ToArray();
            ConnectionManager.SendMessage(bytes);
        }
    }
    
    public virtual void RelayAction(CSteamID userID, RelayedAction relayedAction)
    {
        if (userID == Steam.MySteamID())
        {
            Client.ReceiveRelayedAction(relayedAction);
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.RelayedAction);
            relayedAction.WriteTo(stream);
            byte[] bytes = stream.ToArray();
            ConnectionManager.SendMessage(bytes);
        }
    }

    public virtual void SendGoalAttempt(CSteamID userID, GoalAttempt message)
    {
        if (userID == Steam.MySteamID())
        {
            Client.ReceiveGoalAttempt(message);
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.GoalAttempt);
            message.WriteTo(stream);
            byte[] bytes = stream.ToArray();
            ConnectionManager.SendMessage(bytes);
        }
    }

    public virtual void SendGameEnd(CSteamID userID, GameEnd message)
    {
        if (userID == Steam.MySteamID())
        {
            GameManager.GameEnd(message);
        }
        else
        {
            using MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)MessageType.GameEnd);
            message.WriteTo(stream);
            byte[] bytes = stream.ToArray();
            ConnectionManager.SendMessage(bytes);
        }
    }
    
    // ----------------------------- PROCESS MESSAGES --------------------------------
    public void ProcessMessage(byte[] message)
    {
        IBufferMessage action;
        switch (message[0])
        {
            case (byte)MessageType.Ping:
                PingManager.ReceivePing();
                break;
            case (byte)MessageType.ReplyPing:
                PingManager.ReceiveReplyPing();
                break;
            case (byte)MessageType.PlayerMovementAction:
                action = ParseUtils.Unmarshal<PlayerMovementAction>(message);
                Server.ProcessAction(ConnectionManager.GetPeerID(), action);
                break;
            case (byte)MessageType.PlayerStopAction:
                action = ParseUtils.Unmarshal<PlayerStopAction>(message);
                Server.ProcessAction(ConnectionManager.GetPeerID(), action);
                break;
            case (byte)MessageType.GrabAction:
                action = ParseUtils.Unmarshal<GrabAction>(message);
                Server.ProcessAction(ConnectionManager.GetPeerID(), action);
                break;
            case (byte)MessageType.ThrowAction:
                action = ParseUtils.Unmarshal<ThrowAction>(message);
                Server.ProcessAction(ConnectionManager.GetPeerID(), action);
                break;
            case (byte)MessageType.Ready:
                Server.PeerReady(ConnectionManager.GetPeerID());
                break;
        }
    }
}
