using System;
using Google.Protobuf;
using Steamworks;

public class P2PTestMaster : P2PMaster
{
    private CSteamID DummyID = new CSteamID(1);
    private TimeSpan DummyPing = TimeSpan.Zero;
    public void DummyReady()
    {
        Server.PeerReady(DummyID);
    }

    public void DummySendAction(IBufferMessage action)
    {
        Server.ProcessAction(DummyID, action);
    }

    public override TimeSpan GetPingToUser(CSteamID userID)
    {
        if (userID == DummyID)
        {
            return DummyPing;
        }

        return base.GetPingToUser(userID);
    }

    public void SetDummyPing(TimeSpan ping)
    {
        DummyPing = ping;
    }

    public override TimeSpan GetPing()
    {
        return DummyPing;
    }

    public override void SendGameStart(CSteamID userID)
    {
        if (userID == DummyID)
        {
            return;
        }
        base.SendGameStart(userID);
    }

    public override void SendGameState(CSteamID userID, GameState gameState)
    {
        if (userID == DummyID)
        {
            return;
        }
        base.SendGameState(userID, gameState);
    }
}
