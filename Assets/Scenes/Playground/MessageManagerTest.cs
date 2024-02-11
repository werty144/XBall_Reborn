using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class MessageManagerTest : MessageManagerMaster
{
    private CSteamID DummyID = new CSteamID(1);
    private CSteamID MyID = new CSteamID(0);

    
    // Disable Ping
    public override void SendPing()
    {
        
    }
    
    public override void SendAction(IBufferMessage action)
    {
        Server.ProcessAction(MyID, action);
    }

    // ---------------- IGNORE MESSAGES TO DUMMY ---------------------
    public override void SendGameStart(CSteamID userID)
    {
        if (userID == MyID)
        {
            GameManager.OnGameStart();
        }
    }

    public override void SendGameState(CSteamID userID, GameState gameState)
    {
        if (userID == MyID)
        {
            Client.ReceiveState(gameState);
        }
    }

    public override void SendActionResponse(CSteamID userID, ActionResponse actionResponse)
    {
        if (userID == MyID)
        {
            Client.ReceiveActionResponse(actionResponse);
        }
    }
    
    // --------------------------------- DUMMY ACTIONS -----------------------------
    public void DummyReady()
    {
        Server.PeerReady(DummyID);
    }

    public void DummySendAction(IBufferMessage action)
    {
        Server.ProcessAction(DummyID, action);
    }
}
