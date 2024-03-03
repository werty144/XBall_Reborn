using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;

public class Server : MonoBehaviour, StateHolder
{
    public GameObject PlayerPrefab;
    public GameObject BallPrefab;
    
    protected MessageManagerMaster MessageManager;
    protected PingManager PingManager;
    protected CSteamID[] userIDs = new CSteamID[2];
    private HashSet<CSteamID> PeersReady = new HashSet<CSteamID>();
    
    protected Dictionary<uint, PlayerController> Players = new ();
    protected BallController Ball;
    protected Dictionary<ulong, GoalController> Goal = new();
    private GameStateVersioning GameStateVersioning;
    
    private GameState PausedState;
    private bool OnPause;
    
    private void Awake()
    {
        PlayerPrefab = Resources.Load<GameObject>("Player/ServerPlayerPrefab");
        BallPrefab = Resources.Load<GameObject>("Ball/ServerBallPrefab");
        GameStateVersioning = new GameStateVersioning(this);
    }

    private void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManagerMaster>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
        
        var global = GameObject.FindWithTag("Global");
        var gameStarter = global.GetComponent<GameStarter>();
        userIDs[0] = gameStarter.Info.MyID;
        userIDs[1] = gameStarter.Info.OpponentID;

        CreateInitialState(gameStarter.Info.NumberOfPlayers);
        InitiateGoals();
    }

    private void InitiateGoals()
    {
        foreach (var goal in GameObject.FindGameObjectsWithTag("Goal"))
        {
            var userID = goal.transform.position.z > 0 ? userIDs[1] : userIDs[0];
            var controller = goal.AddComponent<GoalController>();
            controller.UserID = userID.m_SteamID;
            Goal[controller.UserID] = controller;
        }
    }
    
    private void CreateInitialState(int n)
    {
        var collisionLayer = LayerMask.NameToLayer("Server");
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
        
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var player = Instantiate(PlayerPrefab);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            controller.Ball = Ball;
            Players[spareID] = controller;
            spareID++;
            foreach (Renderer renderer in controller.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        
        foreach (var renderer in Ball.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        
        ApplyGameState(InitialState.GetInitialState(n));

        foreach (var playerController in Players.Values)
        {
            foreach (Renderer renderer in playerController.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color;
                        if (playerController.ID % 2 == 0)
                        {
                            color = PlayerConfig.MyColor;
                        }
                        else
                        {
                           color = PlayerConfig.OpponentColor;
                        }

                        color.a = 0.5f;
                        material.color = color;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (OnPause) {return;}
        GameStateVersioning.AddCurrentState();
    }

    public void ProcessAction(CSteamID actorID, IBufferMessage action)
    {
        if (OnPause) {return;}
        
        switch (action)
        {
            case PlayerMovementAction:
                GameStateVersioning.ApplyActionToCurrentState(action);
                MessageManager.SendGameState(GetAnotherID(actorID), GetGameState());
                break;
            case PlayerStopAction:
                GameStateVersioning.ApplyActionToCurrentState(action);
                MessageManager.SendGameState(GetAnotherID(actorID), GetGameState());
                break;  
            case GrabAction grabAction:
                if (grabAction.PreSuccess)
                {
                    GameStateVersioning.ApplyActionToCurrentState(grabAction);
                }
                var relayedGrabAction = new RelayedAction
                {
                    UserId = actorID.m_SteamID,
                    GrabAction = grabAction,
                    Success = grabAction.PreSuccess
                };
                foreach (var userID in userIDs)
                {
                    MessageManager.RelayAction(userID, relayedGrabAction);
                }
                break;
            case ThrowAction throwAction:
                var relayedThrowAction = new RelayedAction
                {
                    UserId = actorID.m_SteamID,
                    ThrowAction = throwAction,
                };
                if (Ball.Owned && Ball.Owner.ID == throwAction.PlayerId)
                {
                    Ball.ThrowTo(ProtobufUtils.FromVector3Protobuf(throwAction.Destination));
                    relayedThrowAction.Success = true;
                }
                else
                {
                    relayedThrowAction.Success = false;
                }
                foreach (var userID in userIDs)
                {
                    MessageManager.RelayAction(userID, relayedThrowAction);
                }
                break;
            default:
                Debug.LogWarning("Unknown action");
                break;
        }
        
        // var actorPing = PingManager.GetPingToUser(actorID);
        // GameStateVersioning.ApplyActionInThePast(action, actorID, actorPing);
        //
        // var currentState = GetGameState();
        // foreach (var userID in userIDs)
        // {
        //     var userPing = PingManager.GetPingToUser(userID);
        //     GameStateVersioning.FastForward(userPing);
        //     var stateForUser = GetGameState();
        //     if (userID == actorID)
        //     {
        //         MessageManager.SendActionResponse(
        //             actorID,
        //             new ActionResponse
        //             {
        //                 ActionId = ParseUtils.GetActionId(action),
        //                 GameState = stateForUser
        //             });
        //     }
        //     else
        //     {
        //         MessageManager.SendGameState(userID, stateForUser);
        //     }
        //
        //     ApplyGameState(currentState);
        // }
    }

    private void BroadCastState()
    {
        var curGameState = GetGameState();
        foreach (var userID in userIDs)
        {
            MessageManager.SendGameState(userID, curGameState);
        }
    }
    
    IEnumerator DelayedAction(int millis, Action action)
    {
        yield return new WaitForSeconds(0.001f * millis);
        action();
    }

    private CSteamID GetAnotherID(CSteamID userID)
    {
        return userIDs[0] == userID ? userIDs[1] : userIDs[0];
    }

    public void PeerReady(CSteamID userID)
    {
        PeersReady.Add(userID);

        if (PeersReady.Count == 2)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        foreach (var user in userIDs)
        {
            MessageManager.SendGameStart(user);
        }
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
    }

    public void PeerDropped()
    {
        Pause();
    }

    private void Pause()
    {
        OnPause = true;
        Physics.autoSimulation = false;
        PausedState = GetGameState();
        StopPlayers();
    }

    private void StopPlayers()
    {
        foreach (var player in GetPlayers().Values)
        {
            player.Stop();
        }
    }

    public void PeerConnected()
    {
        if (OnPause)
        {
            Resume();
        }
    }

    private void Resume()
    {
        OnPause = false;
        Physics.autoSimulation = true;

        ApplyGameState(PausedState);

        foreach (var userID in userIDs)
        {
            MessageManager.SendResumeGame(userID, PausedState);
        }
    }

    public void CollisionExit()
    {
        // TODO: Consider cooldown and periodic sending
        BroadCastState();
    }

    public void OnGoalAttempt(ulong userID)
    {
        var success = GoalRules.GoalAttemptSuccess(Ball, Goal[userID]);
        Debug.Log("Goal attempt! Success: " + success);
        if (success)
        {
            Goal[userID].PlaySuccessAnimation();
        }
        else
        {
            Goal[userID].PlayFailAnimation();
        }
    }
}
