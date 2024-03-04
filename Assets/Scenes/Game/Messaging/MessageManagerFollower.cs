using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Unity.VisualScripting;
using UnityEngine;

public class MessageManagerFollower : MonoBehaviour, MessageManager
{
    private ConnectionManager ConnectionManager;
    private Client Client;
    private GameManager GameManager;
    private PingManager PingManager;
    private void Start()
    {
        ConnectionManager = GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>();
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
    }

    public void SendReady()
    {
        using MemoryStream stream = new MemoryStream();
        stream.WriteByte((byte)MessageType.Ready);
        byte[] bytes = stream.ToArray();
        ConnectionManager.SendMessage(bytes);
    }

    public void SendAction(IBufferMessage action)
    {
        using MemoryStream stream = new MemoryStream();
        switch (action)
        {
            case PlayerMovementAction:
                stream.WriteByte((byte)MessageType.PlayerMovementAction);
                break;
            case PlayerStopAction:
                stream.WriteByte((byte)MessageType.PlayerStopAction);
                break;
            case GrabAction:
                stream.WriteByte((byte)MessageType.GrabAction);
                break;
            case ThrowAction:
                stream.WriteByte((byte)MessageType.ThrowAction);
                break;
            default:
                Debug.LogWarning("Unknown action");
                break;
        }
             
        action.WriteTo(stream);
        byte[] bytes = stream.ToArray();
        ConnectionManager.SendMessage(bytes);
    }

    public void SendPing()
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
            case (byte)MessageType.GameStart:
                GameManager.OnGameStart();
                break;
            case (byte)MessageType.ResumeGame:
                var pausedGameState = ParseUtils.Unmarshal<GameState>(message);
                Client.ReceiveResumeGame(pausedGameState);
                GameManager.ResumeGame();
                break;
            case (byte)MessageType.GameState:
                var gameState = ParseUtils.Unmarshal<GameState>(message);
                Client.ReceiveState(gameState);
                break;
            case (byte)MessageType.RelayedAction:
                var relayedAction = ParseUtils.Unmarshal<RelayedAction>(message);
                Client.ReceiveRelayedAction(relayedAction);
                break;
            case (byte)MessageType.GoalAttempt:
                var goalAttempt = ParseUtils.Unmarshal<GoalAttempt>(message);
                Client.ReceiveGoalAttempt(goalAttempt);
                break;
        }
    }
}
