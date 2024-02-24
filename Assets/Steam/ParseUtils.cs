using System.IO;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Assertions;

public static class ParseUtils
{
    public static T Unmarshal<T>(byte[] message) where T: IMessage<T>, new()
    {
        T output = new T();
        using MemoryStream stream = new MemoryStream(message, 1, message.Length - 1);
        try
        {
            if (output.Descriptor.Parser is MessageParser<T> parser)
            {
                return parser.ParseFrom(stream);
            }
            else
            {
                Debug.LogError("Can not create parser for " + typeof(T));
                return new T();
            }
        }
        catch (InvalidProtocolBufferException e)
        {
            Debug.LogException(e);
            return new T();
        }
    }
    
    public static uint GetActionId(IBufferMessage mbAction)
    {
        switch (mbAction)
        {
            case PlayerMovementAction playerMovementAction:
                return playerMovementAction.ActionId;
            default:
                return 0;
        }
    }
}
