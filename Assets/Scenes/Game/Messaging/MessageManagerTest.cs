using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class MessageManagerTest : MessageManagerMaster
{
    private CSteamID DummyID = new CSteamID(1);

    
    // Disable Ping
    public override void SendPing()
    {
        
    }

    // ---------------- IGNORE MESSAGES TO DUMMY ---------------------
    public override void SendGameStart(CSteamID userID)
    {
        if (userID == Steam.MySteamID())
        {
            GameManager.OnGameStart();
        }
    }

    public override void SendGameState(CSteamID userID, GameState gameState)
    {
        if (userID == Steam.MySteamID())
        {
            Client.ReceiveState(gameState);
        }
    }

    public override void SendActionResponse(CSteamID userID, ActionResponse actionResponse)
    {
        if (userID == Steam.MySteamID())
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
