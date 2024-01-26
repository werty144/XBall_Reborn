using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

struct TimedGameState
{
    private DateTime TimeStamp;
    private GameState GameState;
}

struct TimedAction
{
    private DateTime TimeStamp;
    private IBufferMessage Action;
}

public class GameStateVersioning
{
    private GameManager GameManager;
    
    private static uint BufferSize = 200;
    private BoundedBuffer<TimedGameState> GameStates = new (BufferSize);
    // Only stores server's actions since all client's preceed the last one
    // (assuming we have reliable messaging)
    private BoundedBuffer<TimedAction> Actions = new(BufferSize);

    public GameStateVersioning(GameManager gameManager)
    {
        GameManager = gameManager;
    }
    
    public void AddCurrentState(GameState gameState)
    {
        
    }

    public void AddCurrentAction(IBufferMessage action)
    {
        
    }

    public GameState ApplyActionInThePast(IBufferMessage action, TimeSpan lag)
    {
        // Insert action
        // Calculate point in the past
        // Find closest state
        // Clear states after
        // Until reach presence:
        //  apply physics
        //  apply all actions that apply
        //  Move players
        //  Log state
        
        
        switch (action)
        {
            case PlayerMovementAction playerMovementAction:
                var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
                var player = GameManager.GetPlayers()[playerMovementAction.Id];
                player.SetTarget(target);
                break;
            default:
                Debug.LogWarning("Unknown action");
                break;
        }
        // Physics.autoSimulation = false;
        // while (mergingState < CurStateNumber)
        // {
        //     Physics.Simulate(Time.fixedDeltaTime);
        //     ApplyActions(mergingState);
        //     foreach (var player in Players.Values)
        //     {
        //         player.Move();
        //     }
        //     mergingState++;
        // }
        // Physics.autoSimulation = true;
        return GameManager.GetGameState();
    }
}
