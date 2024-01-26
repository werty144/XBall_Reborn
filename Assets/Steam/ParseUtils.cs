using System.IO;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Assertions;

public static class ParseUtils
{
    public static PlayerMovementAction UnmarshalPlayerMovementAction(byte[] message)
    {
        Assert.AreEqual((byte)MessageType.PlayerMovementAction, message[0]);
        PlayerMovementAction action;
        using (MemoryStream stream = new MemoryStream(message, 1, message.Length - 1))
        {
            try
            {
                action = PlayerMovementAction.Parser.ParseFrom(stream);
            }
            catch (InvalidProtocolBufferException e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        return action;
    }

    public static GameState UnmarshalGameState(byte[] message)
    {
        Assert.AreEqual((byte)MessageType.GameState, message[0]);
        GameState gameState;
        using (MemoryStream stream = new MemoryStream(message, 1, message.Length - 1))
        {
            try
            {
                gameState = GameState.Parser.ParseFrom(stream);
            }
            catch (InvalidProtocolBufferException e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        return gameState;
    }
}
