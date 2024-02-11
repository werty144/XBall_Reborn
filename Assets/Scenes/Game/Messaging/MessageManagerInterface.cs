using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public enum MessageType
{
    PlayerMovementAction = 1,
    Ready = 2,
    Ping = 3,
    ReplyPing = 4,
    GameState = 5,
    GameStart = 6,
    ActionResponse = 7,
    PlayerStopAction = 8
}

public interface MessageManager
{
    public void SendReady();
    public void SendAction(IBufferMessage action);
    public void SendPing();
    public void SendReplyPing();

    public void ProcessMessage(byte[] message);
}
