using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using IngameDebugConsole;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Update = UnityEngine.PlayerLoop.Update;

public class Client : MonoBehaviour, StateHolder
{
    public GameObject PlayerPrefab;
    public GameObject BallPrefab;

    protected MessageManager MessageManager;
    
    protected Dictionary<uint, PlayerController> Players = new();
    protected BallController Ball;
    protected GameStateVersioning GameStateVersioning;
    private uint NextActionId = 1;
    private Dictionary<uint, Stopwatch> ActionTimers = new();
    
    public Dictionary<uint, PlayerController> ServerPlayers = new();
    public BallController ServerBall;

    protected ulong MyID;

    protected virtual void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;

        MyID = setupInfo.MyID.m_SteamID;
        
        CreateInitialState(setupInfo.NumberOfPlayers, setupInfo.IAmMaster);
        CreateServerState(setupInfo.NumberOfPlayers, LayerMask.NameToLayer("ClientServer"));
        
        GameStateVersioning = new GameStateVersioning(this);
    }

    protected virtual void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
    }

    private void FixedUpdate()
    {
        InterpolateToServerState();
    }

    private void CreateInitialState(int n, bool IAmMaster)
    {
        int collisionLayer = LayerMask.NameToLayer("Client");
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var player = Instantiate(PlayerPrefab);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            Players[spareID] = controller;
            spareID++;
        }
        foreach (var player in Players.Values)
        {
            player.IsMy = IAmMaster ^ (player.ID % 2 == 1);
            player.Colorize(player.IsMy ? PlayerConfig.MyColor : PlayerConfig.OpponentColor);
        }
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
        
        ApplyGameState(InitialState.GetInitialState(n));
    }

    protected void CreateServerState(int n, int collisionLayer)
    {
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var player = Instantiate(PlayerPrefab);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            ServerPlayers[spareID] = controller;
            spareID++;
            foreach (Renderer renderer in controller.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        ServerBall = ballObject.GetComponent<BallController>();
        foreach (var renderer in ServerBall.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        
        ApplyServerState(InitialState.GetInitialState(n));
    }

    public void InputAction(IBufferMessage action)
    {
        PlayerController player;
        switch (action)
        {
            case PlayerMovementAction playerMovementAction:
                player = Players[playerMovementAction.PlayerId];
                if (!player.IsMy) { return; }
                
                var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
                player.SetMovementTarget(target);

                playerMovementAction.ActionId = NextActionId;
                NextActionId++;
                break;
            case PlayerStopAction playerStopAction:
                player = Players[playerStopAction.PlayerId];
                if (!player.IsMy) { return; }

                player.Stop();
                playerStopAction.ActionId = NextActionId;
                NextActionId++;
                break;
            case GrabAction grabAction:
                player = Players[grabAction.PlayerId];
                if (!player.IsMy) { return; }
                player.PlayGrabAnimation();
                if (!ActionRules.IsValidGrab(player.transform, Ball.transform))
                {
                    return;
                }
                grabAction.ActionId = NextActionId;
                ActionTimers[grabAction.ActionId] = Stopwatch.StartNew();
                NextActionId++;
                break;
            case ThrowAction throwAction:
                if (!Ball.Owned || !Ball.Owner.IsMy)
                {
                    return;
                }
                var initialTarget = ProtobufUtils.FromVector3Protobuf(throwAction.Destination);
                var resultingTarget = ActionRules.CalculateThrowTarget(Ball.Owner, initialTarget);
                Instantiate(Resources.Load<GameObject>("TargetMarker/EventualTarget"), resultingTarget,
                    Quaternion.identity);
                throwAction.Destination = ProtobufUtils.ToVector3ProtoBuf(resultingTarget);
                throwAction.PlayerId = Ball.Owner.ID;
                Ball.Owner.PlayThroughAnimation();
                throwAction.ActionId = NextActionId;
                ActionTimers[throwAction.ActionId] = Stopwatch.StartNew();
                NextActionId++;
                break;
            default:
                Debug.LogWarning("Unknown input action");
                break;
        }
        
        MessageManager.SendAction(action);
    }

    public void ReceiveResumeGame(GameState gameState)
    {
        ApplyGameState(gameState);
    }

    public void ReceiveState(GameState gameState)
    {
        ApplyServerState(gameState);
    }

    private void InterpolateToServerState()
    {
        foreach (var player in Players.Values)
        {
            if (player.IsMy)
            {
                continue;
            }
            player.InterpolateToState(ServerPlayers[player.ID]);
        }

        if (!Ball.Owned && !ServerBall.Owned)
        {
            Ball.InterpolateToState(ServerBall);
        }
    }

    public void ReceiveRelayedAction(RelayedAction relayedAction)
    {
        if (relayedAction.UserId == MyID)
        {
            if (relayedAction.Success == false)
            {
                return;
            }

            Stopwatch timer;
            long timePassed;
            int timeLeft;
            switch (relayedAction.ActionCase)
            {
                case RelayedAction.ActionOneofCase.GrabAction:
                    ServerBall.SetOwner(ServerPlayers[relayedAction.GrabAction.PlayerId]);
                    timer = ActionTimers[relayedAction.GrabAction.ActionId];
                    ActionTimers.Remove(relayedAction.GrabAction.ActionId);
                    timer.Stop();
                    timePassed = timer.ElapsedMilliseconds;
                    timeLeft = Mathf.Max(0, ActionRulesConfig.GrabDuration - (int)timePassed);
                    StartCoroutine(DelayedAction(timeLeft,
                        () =>
                        {
                            var newOwner = Players[relayedAction.GrabAction.PlayerId];
                            if (ServerBall.Owned && ServerBall.Owner.ID == relayedAction.GrabAction.PlayerId)
                            {
                                Ball.SetOwner(newOwner);
                            }
                        }));
                    break;
                case RelayedAction.ActionOneofCase.ThrowAction:
                    var ballTarget = ProtobufUtils.FromVector3Protobuf(relayedAction.ThrowAction.Destination);
                    ServerBall.ThrowTo(ballTarget);
                    timer = ActionTimers[relayedAction.ThrowAction.ActionId];
                    ActionTimers.Remove(relayedAction.ThrowAction.ActionId);
                    timer.Stop();
                    timePassed = timer.ElapsedMilliseconds;
                    timeLeft = Mathf.Max(0, ActionRulesConfig.ThrowDuration - (int)timePassed);
                    StartCoroutine(DelayedAction(timeLeft,
                        () =>
                        {
                            Ball.ThrowTo(ballTarget);
                        }));
                    break;
            }
        }
        else
        {
            PlayerController player;
            switch (relayedAction.ActionCase)
            {
                case RelayedAction.ActionOneofCase.GrabAction:
                    player = Players[relayedAction.GrabAction.PlayerId];
                    player.PlayGrabAnimation();
                    if (relayedAction.Success)
                    {
                        ServerBall.SetOwner(ServerPlayers[relayedAction.GrabAction.PlayerId]);
                        StartCoroutine(DelayedAction(ActionRulesConfig.GrabDuration,
                            () =>
                            {
                                Ball.SetOwner(Players[relayedAction.GrabAction.PlayerId]);
                            }));
                    }
                    break;
                case RelayedAction.ActionOneofCase.ThrowAction:
                    player = Players[relayedAction.ThrowAction.PlayerId];
                    player.PlayThroughAnimation();
                    if (relayedAction.Success)
                    {
                        var ballTarget = ProtobufUtils.FromVector3Protobuf(relayedAction.ThrowAction.Destination);
                        ServerBall.ThrowTo(ballTarget);
                        StartCoroutine(DelayedAction(ActionRulesConfig.ThrowDuration,
                            () =>
                            {
                                Ball.ThrowTo(ballTarget);
                            }));
                    }
                    break;
            }
        }
    }
    
    IEnumerator DelayedAction(int millis, Action action)
    {
        yield return new WaitForSeconds(0.001f * millis);
        action();
    }
    
    public GameState GetGameState()
    {
        GameState gameState = new GameState();
        foreach (var player in Players.Values)
        { 
            gameState.PlayerStates.Add(player.GetState());
        }

        gameState.BallState = Ball.GetState();

        return gameState;
    }
    
    public Dictionary<uint, PlayerController> GetPlayers()
    {
        return Players;
    }

    public BallController GetBall()
    {
        return Ball;
    }
    
    public void ApplyGameState(GameState state)
    {
        foreach (var playerState in state.PlayerStates)
        {
            Players[playerState.Id].ApplyState(playerState);
        }
        Ball.ApplyState(state.BallState);
    }

    public void ApplyServerState(GameState state)
    {
        foreach (var playerState in state.PlayerStates)
        {
            ServerPlayers[playerState.Id].ApplyState(playerState);
        }

        ServerBall.ApplyState(state.BallState);
    }
}
