using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Steamworks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Assertions;
using Vector2 = UnityEngine.Vector2;

public class GameStateVersioning
{
    private StateHolder StateHolder;
    
    private static uint BufferSize = 200;
    private StateBuffer GameStates;
    // Only stores server's actions since all client's preceed the last one
    // (assuming we have reliable messaging)
    private ActionBuffer Actions;

    private Dictionary<CSteamID, DateTime> LastAppliedActionTimes = new ();
    private uint CurrentStateNumber;

    public GameStateVersioning(StateHolder stateHolder)
    {
        StateHolder = stateHolder;
        CurrentStateNumber = 0;
        Actions = new(BufferSize);
        GameStates = new(BufferSize);
    }
    
    public void AddCurrentState()
    {
        GameStates.Insert(StateHolder.GetGameState(), CurrentStateNumber, DateTime.Now, Time.deltaTime);
        CurrentStateNumber++;
    }

    private DateTime GetLastAppliedActionTime(CSteamID actorID)
    {
        if (!LastAppliedActionTimes.ContainsKey(actorID))
        {
            return DateTime.Now - TimeSpan.FromHours(1);
        }

        return LastAppliedActionTimes[actorID];
    }

    public void AddCurrentAction(IBufferMessage action)
    {
        Actions.Insert(action, CurrentStateNumber);
    }

    public void ApplyActionInThePast(IBufferMessage action, CSteamID actorID, TimeSpan lag)
    {
        // Calculate point in the past (account for the previous action, should be after)
        // Find closest state
        // Clear states after
        // Until reach presence:
        //  apply physics
        //  apply all actions that apply
        //  Move players
        //  Log state

        var initialState = StateHolder.GetGameState();

        var currentTime = DateTime.Now;
        var actionTime = currentTime - lag;

        var lastAppliedActionTime = GetLastAppliedActionTime(actorID);
        if (actionTime <= lastAppliedActionTime)
        {
            Debug.LogWarning("Inverse order of follower actions. Wrong ping estimation");
            actionTime = lastAppliedActionTime + TimeSpan.FromMilliseconds(1);
        }

        LastAppliedActionTimes[actorID] = actionTime;

        var (actionState, actionStateIndex) = GameStates.FindFirstAfterTime(actionTime);
        if (actionState == null)
        {
            ApplyActionToCurrentState(action);
            Actions.Insert(action, CurrentStateNumber + 1);
            return;
        }
        
        StateHolder.ApplyGameState(actionState.Value.GameState);
        ApplyActionToCurrentState(action);
        Actions.Insert(action, actionState.Value.StateNumber);
        var updatedState = new TimedGameState
        {
            GameState = StateHolder.GetGameState(),
            StateNumber = actionState.Value.StateNumber,
            TimeStamp = actionState.Value.TimeStamp,
            DeltaTime = actionState.Value.DeltaTime
        };
        Assert.IsTrue(TestEqual(updatedState, actionState.Value), "Initially equal");
        GameStates.Set(actionStateIndex, updatedState);

        Physics.autoSimulation = false;
        var currentStateIndex = actionStateIndex;
        var currentState = updatedState;
        while (GameStates.Has(currentStateIndex + 1))
        {
            var nextRecordedState = GameStates.Get(currentStateIndex + 1);
            
            // Apply modifications to the current state
            // Order matters
            Physics.Simulate(nextRecordedState.DeltaTime);
            ApplyMovementToCurrentState(nextRecordedState.DeltaTime);
            var currentActions = Actions.GetAction(nextRecordedState.StateNumber);
            foreach (var currentAction in currentActions)
            {
                ApplyActionToCurrentState(currentAction);
            }

            currentStateIndex++;
            currentState = new TimedGameState
            {
                GameState = StateHolder.GetGameState(),
                StateNumber = nextRecordedState.StateNumber,
                TimeStamp = nextRecordedState.TimeStamp,
                DeltaTime = nextRecordedState.DeltaTime
            };
            GameStates.Set(currentStateIndex, currentState);
        }
        // Since this code runs after physics was simulated and movements were applied
        // in the frame and we discarded it. Relies on the script execution order!!!
        Physics.Simulate(Time.deltaTime);
        ApplyMovementToCurrentState(Time.deltaTime);
        Physics.autoSimulation = true;
        
        SmoothFromPast(initialState);
    }

    public void ApplyActionToCurrentState(IBufferMessage action)
    {
        PlayerController player;
        switch (action)
        {
            case PlayerMovementAction playerMovementAction:
                var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
                player = StateHolder.GetPlayers()[playerMovementAction.PlayerId];
                player.SetMovementTarget(target);
                break;
            case PlayerStopAction playerStopAction:
                player = StateHolder.GetPlayers()[playerStopAction.PlayerId];
                player.Stop();
                break;
            case GrabAction grabAction:
                player = StateHolder.GetPlayers()[grabAction.PlayerId];
                var ball = StateHolder.GetBall();
                ball.SetOwner(player);
                break;
            default:
                Debug.LogWarning("Unknown action");
                break;
        }
    }

    private void ApplyMovementToCurrentState(float deltaTime)
    {
        foreach (var player in StateHolder.GetPlayers().Values)
        {
            player.Move(deltaTime);
        }
    }

    public bool TestEqual(TimedGameState state1, TimedGameState state2)
    {
        for (int i = 0; i < state1.GameState.PlayerStates.Count; i += 2)
        {
            var playerState1 = state1.GameState.PlayerStates[i];
            var playerState2 = state2.GameState.PlayerStates[i];
            if (Math.Abs(playerState1.X - playerState2.X) > 0.0001f ||
                Math.Abs(playerState1.Y - playerState2.Y) > 0.0001f)
            {
                Debug.Log(state1.GameState.PlayerStates[i].ToString());
                Debug.Log(state2.GameState.PlayerStates[i].ToString());
                return false;
            }
        }

        return true;
    }

    public void SmoothFromPast(GameState pastState)
    {
        // Modifies current state so that objects that are close to their
        // position in the past stay as they were.
        // Preserves targets from the current state

        foreach (var currentPlayer in StateHolder.GetPlayers().Values)
        {
            foreach (var oldPlayer in pastState.PlayerStates)
            {
                if (oldPlayer.Id == currentPlayer.ID)
                {
                    var currentPosition = currentPlayer.GetPosition();
                    var oldPosition = new Vector2(oldPlayer.X, oldPlayer.Y);
                    if (Vector2.Distance(currentPosition, oldPosition) <= PlayerConfig.Radius)
                    {
                        currentPlayer.SetPosition(oldPosition);
                    }
                    break;
                }
            }
        }
    }

    public void FastForward(TimeSpan lag)
    {
        if (lag == TimeSpan.Zero)
        {
            return;
        }
        var nIterations = 10;
        var deltaTime = (float)(lag / nIterations).TotalSeconds;
        Physics.autoSimulation = false;
        for (int i = 0; i < nIterations; i++)
        {
            Physics.Simulate(deltaTime);
            ApplyMovementToCurrentState(deltaTime);
            
            // Don't need to log since called only on the follower
        }

        Physics.autoSimulation = true;
    }
}

public class ActionBuffer
{
    // At most one action per state
    
    private uint BufferSize;

    private Dictionary<uint, List<IBufferMessage>> StatesToActions;

    public ActionBuffer(uint bufferSize)
    {
        BufferSize = bufferSize;
        StatesToActions = new Dictionary<uint, List<IBufferMessage>>();
    }

    public void Insert(IBufferMessage action, uint stateNumber)
    {
        if (!StatesToActions.ContainsKey(stateNumber))
        {
            StatesToActions[stateNumber] = new List<IBufferMessage>();
        }
        StatesToActions[stateNumber].Add(action);
        if (StatesToActions.Count > BufferSize)
        {
            StatesToActions.Remove(StatesToActions.Keys.Min());
        }
    }

    public List<IBufferMessage> GetAction(uint stateNumber)
    {
        if (!StatesToActions.ContainsKey(stateNumber))
        {
            return new List<IBufferMessage>();
        }

        return StatesToActions[stateNumber];
    }
}

public struct TimedGameState
{
    public DateTime TimeStamp;
    public float DeltaTime;
    public uint StateNumber;
    public GameState GameState;
}

public class StateBuffer
{
    // What do we need from the state structure
    // 1. Insert
    // 2. Keep bounded size (remove oldest)
    // 3. Find closest by time
    // 4. GetNext
    // 5. HasNext
    // 6. Overwrite
    
    private uint BufferSize;
    private TimedGameState[] States;
    private int NextSlot;
    private uint Counter;
    
    public StateBuffer(uint bufferSize)
    {
        BufferSize = bufferSize;
        NextSlot = 0;
        States = new TimedGameState[BufferSize];
        Counter = 0;
    }

    public void Insert(GameState state, uint stateNumber, DateTime timeStamp, float deltaTime)
    {
        Counter++;
        var timedState = new TimedGameState
        {
            GameState = state,
            StateNumber = stateNumber,
            TimeStamp = timeStamp,
            DeltaTime = deltaTime
        };
        States[NextSlot] = timedState;
        NextSlot++;
        if (NextSlot == BufferSize)
        {
            NextSlot = 0;
        }
    }

    public (TimedGameState?, uint) FindFirstAfterTime(DateTime timeStamp)
    {
        uint nextInd = 0;
        while (Has(nextInd))
        {
            var state = Get(nextInd);
            if (state.TimeStamp >= timeStamp)
            {
                return (state, nextInd);
            }

            nextInd++;
        }

        return (null, BufferSize);
    }

    public bool Has(uint i)
    {
        if (Counter <= i)
        {
            return false;
        }

        return i <= BufferSize - 1;
    }

    public TimedGameState Get(uint i)
    {
        // Make sure to call Has before
        return States[(NextSlot + i) % BufferSize];
    }

    public void Set(uint i, TimedGameState state)
    {
        States[(NextSlot + i) % BufferSize] = state;
    }
}
