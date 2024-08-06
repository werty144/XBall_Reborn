using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Google.Protobuf;
using IngameDebugConsole;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Update = UnityEngine.PlayerLoop.Update;

public class Client : MonoBehaviour, StateHolder
{
    public GameObject ClientPlayerPrefabBlue;
    public GameObject ClientPlayerPrefabRed;
    public GameObject BasePlayerPrefab;
    public GameObject BallPrefab;
    public InputManager InputManager;

    public ScorePanelController ScorePanelController;

    protected MessageManager MessageManager;
    
    protected Dictionary<uint, PlayerController> Players = new();
    protected BallController Ball;
    protected Dictionary<ulong, GameObject> Goals = new();
    protected GameStateVersioning GameStateVersioning;
    
    private uint NextActionId = 1;
    private Dictionary<uint, Stopwatch> ActionTimers = new();
    public ActionScheduler ActionScheduler;
    
    public Dictionary<uint, PlayerController> ServerPlayers = new();
    public BallController ServerBall;

    private Dictionary<uint, float> NextGrabTime = new();

    protected ulong MyID;
    protected ulong OpponentID;

    protected virtual void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;

        MyID = setupInfo.MyID.m_SteamID;
        OpponentID = setupInfo.OpponentID.m_SteamID;
        
        CreateInitialState(setupInfo.NumberOfPlayers, setupInfo.IAmMaster);
        InitiateCooldowns();
        InitiateGoals();
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

    protected void InitiateGoals()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;
        foreach (var goal in GameObject.FindGameObjectsWithTag("Goal"))
        {
            if (goal.transform.position.z < 0 && setupInfo.IAmMaster || goal.transform.position.z > 0 && !setupInfo.IAmMaster)
            {
                Goals[setupInfo.MyID.m_SteamID] = goal;
            }
            else
            {
                Goals[setupInfo.OpponentID.m_SteamID] = goal;
                var probabilityController = goal.GetComponent<GoalProbabilityController>();
                probabilityController.Client = this;
                probabilityController.MyUserID = MyID;
            }
        }
    }

    protected void InitiateCooldowns()
    {
        foreach (var player in Players.Values)
        {
            if (!player.IsMy)
            {
                continue;
            }

            NextGrabTime[player.ID] = 0;
        }
    }

    private void CreateInitialState(int n, bool IAmMaster)
    {
        int collisionLayer = LayerMask.NameToLayer("Client");
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var isMy = IAmMaster ^ (i % 2 == 1);
            var player = Instantiate(isMy ? ClientPlayerPrefabBlue : ClientPlayerPrefabRed);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            controller.Ball = Ball;
            Players[spareID] = controller;
            spareID++;
        }

        int sparePlayerNumber = 1;
        foreach (var player in Players.Values)
        {
            player.IsMy = IAmMaster ^ (player.ID % 2 == 1);
            player.UserID = player.IsMy ? MyID : OpponentID;
            if (player.IsMy)
            {
                player.GetComponent<SelectionManager>().PlayerNumber = sparePlayerNumber;
                sparePlayerNumber++;
            }
        }
        
        ApplyGameState(InitialState.GetInitialState(n));
    }

    protected void CreateServerState(int n, int collisionLayer)
    {
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        ServerBall = ballObject.GetComponent<BallController>();
        
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var player = Instantiate(BasePlayerPrefab);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            controller.Ball = ServerBall;
            ServerPlayers[spareID] = controller;
            spareID++;
            foreach (Renderer renderer in controller.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        
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
                // if (!player.IsMy) { return; }
                
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
                grabAction.PreSuccess = ActionRules.BallGrabSuccess(player, Ball);
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
                throwAction.Destination = ProtobufUtils.ToVector3ProtoBuf(resultingTarget);
                throwAction.PlayerId = Ball.Owner.ID;
                Ball.Owner.PlayThroughAnimation();
                throwAction.ActionId = NextActionId;
                ActionTimers[throwAction.ActionId] = Stopwatch.StartNew();
                NextActionId++;
                var goalSuccess = GoalRules.GoalAttemptSuccess(Players, Ball.Owner, Ball, Goals[OpponentID]);
                throwAction.GoalSuccess = goalSuccess;
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
            
            switch (relayedAction.ActionCase)
            {
                case RelayedAction.ActionOneofCase.GrabAction:
                    ServerBall.SetOwner(ServerPlayers[relayedAction.GrabAction.PlayerId]);
                    var timeLeft = ManageTimer(relayedAction.GrabAction.ActionId);
                    ActionScheduler.Schedule(() =>
                    {
                        Ball.SetOwner(Players[relayedAction.GrabAction.PlayerId]);
                    }, timeLeft);
                    break;
                case RelayedAction.ActionOneofCase.ThrowAction:
                    var ballTarget = ProtobufUtils.FromVector3Protobuf(relayedAction.ThrowAction.Destination);
                    ServerBall.ThrowTo(ballTarget);
                    timeLeft = ManageTimer(relayedAction.ThrowAction.ActionId);
                    ActionScheduler.Schedule(() =>
                    {
                        Players[relayedAction.ThrowAction.PlayerId].GetComponent<GrabManager>().SetCooldownMillis(2000f);
                        Ball.ThrowTo(ballTarget);
                    }, timeLeft);
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
                        ActionScheduler.Schedule(() =>
                        {
                            Ball.SetOwner(Players[relayedAction.GrabAction.PlayerId]);
                        }, ActionRulesConfig.GrabDuration);
                    }
                    break;
                case RelayedAction.ActionOneofCase.ThrowAction:
                    player = Players[relayedAction.ThrowAction.PlayerId];
                    player.PlayThroughAnimation();
                    if (relayedAction.Success)
                    {
                        var ballTarget = ProtobufUtils.FromVector3Protobuf(relayedAction.ThrowAction.Destination);
                        ServerBall.ThrowTo(ballTarget);
                        ActionScheduler.Schedule(() =>
                        {
                            Ball.ThrowTo(ballTarget);
                        }, ActionRulesConfig.GrabDuration);
                    }
                    break;
            }
        }
    }

    int ManageTimer(uint actionId)
    {
        var timer = ActionTimers[actionId];
        ActionTimers.Remove(actionId);
        timer.Stop();
        var timePassed = timer.ElapsedMilliseconds;
        var timeLeft = Mathf.Max(0, ActionRulesConfig.GrabDuration - (int)timePassed);
        return timeLeft;
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

    public virtual void ReceiveGoalAttempt(GoalAttempt goalAttempt)
    {
        ScorePanelController.OnGoalAttempt(goalAttempt);
        Goals[goalAttempt.GoalOwner].GetComponentInChildren<Animator>()
            .Play(goalAttempt.Success ? "success" : "failure", -1, 0f);
        
        if (goalAttempt.Success)
        {
            Players[goalAttempt.ThrowerId].gameObject.GetComponent<Pig>().Piggiwise();
        }
    }

    public void GaolShotInput(uint playerID)
    {
        var throwAction = new ThrowAction
        {
            PlayerId = playerID,
            Destination = ProtobufUtils.ToVector3ProtoBuf(Goals[OpponentID].transform.position)
        };
        InputAction(throwAction);
    }

    
}
