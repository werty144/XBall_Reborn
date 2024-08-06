using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class MessageManagerTest : MessageManagerMaster
{
    private CSteamID DummyID = new CSteamID(1);
    private CSteamID MyID = new CSteamID(0);
    private TimeSpan MyPing = TimeSpan.Zero;
    private TimeSpan DummyPing = TimeSpan.Zero;

    
    // Disable Ping
    public override void SendPing()
    {
        
    }
    
    public void SetMyPing(int millis)
    {
        MyPing = TimeSpan.FromMilliseconds(millis);
    }
    
    public void SetDummyPing(int millis)
    {
        DummyPing = TimeSpan.FromMilliseconds(millis);
    }
    
    public override void SendAction(IBufferMessage action)
    {
        StartCoroutine(
            DelayedAction(
                MyPing,
            () => Server.ProcessAction(MyID, action)
            )
            );
    }

    public override void SendGameStart(CSteamID userID)
    {
        if (userID == MyID)
        {
            StartCoroutine(
                DelayedAction(
                    MyPing,
                    () => GameManager.OnGameStart()
                )
            );
        }
    }
    
    public override void SendGameEnd(CSteamID userID, GameEnd message)
    {
        if (userID == MyID)
        {
            StartCoroutine(
                DelayedAction(
                    MyPing,
                    () => GameManager.GameEnd(message)
                )
            );
        }
    }

    public override void SendGameState(CSteamID userID, GameState gameState)
    {
        if (userID == MyID)
        {
            StartCoroutine(
                DelayedAction(
                    MyPing,
                    () => Client.ReceiveState(gameState)
                )
            );
        }
        else
        {
            var dummy = GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>();
            StartCoroutine(
                DelayedAction(
                    DummyPing,
                    () => dummy.ReceiveState(gameState)
                )
            );
        }
    }

    public override void RelayAction(CSteamID userID, RelayedAction relayedAction)
    {
        if (userID == MyID)
        {
            StartCoroutine(
                DelayedAction(
                    MyPing,
                    () => Client.ReceiveRelayedAction(relayedAction)
                )
            );
        }
        else
        {
            var dummy = GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>();
            StartCoroutine(
                DelayedAction(
                    DummyPing,
                    () => dummy.ReceiveRelayedAction(relayedAction)
                )
            );
        }
    }

    public override void SendGoalAttempt(CSteamID userID, GoalAttempt message)
    {
        if (userID == MyID)
        {
            StartCoroutine(
                DelayedAction(
                    MyPing,
                    () => Client.ReceiveGoalAttempt(message)
                )
            );
        }
        else
        {
            var dummy = GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>();
            StartCoroutine(
                DelayedAction(
                    DummyPing,
                    () => dummy.ReceiveGoalAttempt(message)
                )
            );
        }
    }

    // --------------------------------- DUMMY ACTIONS -----------------------------
    public void DummyReady()
    {
        StartCoroutine(
            DelayedAction(
                DummyPing,
                () => Server.PeerReady(DummyID)
            )
        );
    }

    public void DummySendAction(IBufferMessage action)
    {
        StartCoroutine(
            DelayedAction(
                DummyPing,
                () => Server.ProcessAction(DummyID, action)
            )
        );
    }

    IEnumerator DelayedAction(TimeSpan delay, Action action)
    {
        yield return new WaitForSeconds((float)delay.TotalSeconds);
        action();
    }
}
