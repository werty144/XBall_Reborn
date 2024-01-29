using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class P2PFollower : P2PBase
{
    public override void ConnectToServer(CSteamID serverID)
    {
        var identity = new SteamNetworkingIdentity();
        identity.SetSteamID(serverID);
        SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, null);
    }

    public override void SendAction(IBufferMessage action)
    {
        using MemoryStream stream = new MemoryStream();
        switch (action)
        {
            case PlayerMovementAction:
                stream.WriteByte((byte)MessageType.PlayerMovementAction);
                break;
            default:
                Debug.LogWarning("Unknown action");
                break;
        }
             
        action.WriteTo(stream);
        byte[] bytes = stream.ToArray();
        SendMessage(bytes);
    }

    public override void SendReady()
    {
        using MemoryStream stream = new MemoryStream();
        stream.WriteByte((byte)MessageType.Ready);
        byte[] bytes = stream.ToArray();
        SendMessage(bytes);
    }

    protected override void ProcessMessage(byte[] message)
    {
        base.ProcessMessage(message);
        switch (message[0])
        {
            case (byte)MessageType.GameStart:
                Client.OnGameStart();
                break;
            case (byte)MessageType.GameState:
                var gameState = ParseUtils.UnmarshalGameState(message);
                Client.ReceiveState(gameState);
                break;
            case (byte)MessageType.ActionResponse:
                var actionResponse = ParseUtils.UnmarshalActionResponse(message);
                Client.ReceiveActionResponse(actionResponse);
                break;
        }
    }
}
