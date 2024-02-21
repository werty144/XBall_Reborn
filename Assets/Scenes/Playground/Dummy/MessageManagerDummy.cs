using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class MessageManagerDummy : MessageManagerTest
{
    private MessageManagerTest MM;
    private void OnEnable()
    {
        MM = GameObject.FindWithTag("P2P").GetComponent<MessageManagerTest>();
    }

    public override void SendAction(IBufferMessage action)
    {
        MM.DummySendAction(action);
    }
}
