using System;
using System.Collections;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class P2PTestMaster : P2PMaster
{
    private CSteamID DummyID = new CSteamID(1);
    private TimeSpan DummyPing = TimeSpan.Zero;
    private TimeSpan MyPing = TimeSpan.FromMilliseconds(1000);
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

        return MyPing;
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
        StartCoroutine(DelayedAction(() => base.SendGameState(userID, gameState)));
    }

    public override void SendAction(IBufferMessage action)
    {
        // base.SendAction(action);
        StartCoroutine(DelayedAction(() => base.SendAction(action)));
    }

    IEnumerator DelayedAction(Action callback)
    {
        yield return new WaitForSeconds((float)MyPing.TotalSeconds);
        callback();
    }
}
