using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;

struct LastThrowInfo
{
    public bool GoalSuccess;
    public uint PlayerID;

    public LastThrowInfo(bool goalSuccess, uint playerID)
    {
        GoalSuccess = goalSuccess;
        PlayerID = playerID;
    }
}

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
    private ActionScheduler ActionScheduler;
    private BallStateManager BallStateManager = new();

    private LastThrowInfo LastThrow;
    
    private GameState PausedState;
    private bool OnPause;

    private Dictionary<ulong, int> Score = new();
    
    private void Awake()
    {
        PlayerPrefab = Resources.Load<GameObject>("Player/ServerPlayerPrefab");
        BallPrefab = Resources.Load<GameObject>("Ball/ServerBallPrefab");
        ActionScheduler = GetComponent<ActionScheduler>();
    }

    private void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManagerMaster>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
        
        var global = GameObject.FindWithTag("Global");
        var gameStarter = global.GetComponent<GameStarter>();
        userIDs[0] = gameStarter.Info.MyID;
        userIDs[1] = gameStarter.Info.OpponentID;

        Score[userIDs[0].m_SteamID] = 0;
        Score[userIDs[1].m_SteamID] = 0;

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
            controller.UserID = userIDs[i % 2].m_SteamID;
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
    
    public void ProcessAction(CSteamID actorID, IBufferMessage action)
    {
        if (OnPause) {return;}
        
        var pingToActor = PingManager.GetPingToUser(actorID).Milliseconds;
        bool success;
        switch (action)
        {
            case PlayerMovementAction playerMovementAction:
                var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
                var player = Players[playerMovementAction.PlayerId];
                player.SetMovementTarget(target);
                MessageManager.SendGameState(GetAnotherID(actorID), GetGameState());
                break;
            case PlayerStopAction playerStopAction:
                player = Players[playerStopAction.PlayerId];
                player.Stop();
                MessageManager.SendGameState(GetAnotherID(actorID), GetGameState());
                break;  
            case GrabAction grabAction:
                success = grabAction.PreSuccess && BallStateManager.Grab(Players[grabAction.PlayerId]);
                if (success)
                {
                    var delay = Math.Max(0, ActionRulesConfig.GrabDuration - pingToActor);
                    ActionScheduler.Schedule(() =>
                    {
                        Ball.SetOwner(Players[grabAction.PlayerId]);
                        BroadcastState();
                    }, delay);
                }
                var relayedGrabAction = new RelayedAction
                {
                    UserId = actorID.m_SteamID,
                    GrabAction = grabAction,
                    Success = success
                };
                foreach (var userID in userIDs)
                {
                    MessageManager.RelayAction(userID, relayedGrabAction);
                }
                break;
            case ThrowAction throwAction:
                success = BallStateManager.Throw(Players[throwAction.PlayerId]);
                if (success)
                {
                    var delay = Math.Max(0, ActionRulesConfig.ThrowDuration - pingToActor);
                    ActionScheduler.Schedule(() =>
                    {
                        Ball.ThrowTo(ProtobufUtils.FromVector3Protobuf(throwAction.Destination));
                        LastThrow = new LastThrowInfo(throwAction.GoalSuccess, throwAction.PlayerId);
                        BroadcastState();
                    }, delay);
                }

                var relayedThrowAction = new RelayedAction
                {
                    UserId = actorID.m_SteamID,
                    ThrowAction = throwAction,
                    Success = success
                };
                foreach (var userID in userIDs)
                {
                    MessageManager.RelayAction(userID, relayedThrowAction);
                }
                break;
            default:
                Debug.LogWarning("Unknown action");
                break;
        }
    }

    private void BroadcastState()
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
        BroadcastState();
    }

    public void OnGoalAttempt(ulong goalOwner)
    {
        if (LastThrow.GoalSuccess)
        {
            Score[GetAnotherID(new CSteamID(goalOwner)).m_SteamID]++;
        }
        var message = new GoalAttempt
        {
            GoalOwner = goalOwner,
            Success = LastThrow.GoalSuccess,
            Score = { Score },
            ThrowerId = LastThrow.PlayerID
        };
        foreach (var userID in userIDs)
        {
            MessageManager.SendGoalAttempt(userID, message);
        }
        
        CheckForWin();
    }

    void CheckForWin()
    {
        var scoreDiffToWin = 3;
        var gameEndMessage = new GameEnd
        {
            Score = { Score }
        };
        if (Score[userIDs[0].m_SteamID] >= Score[userIDs[1].m_SteamID] + scoreDiffToWin)
        {
            gameEndMessage.Winner = userIDs[0].m_SteamID;
            MessageManager.SendGameEnd(userIDs[0], gameEndMessage);
            MessageManager.SendGameEnd(userIDs[1], gameEndMessage);
        }
        if (Score[userIDs[1].m_SteamID] >= Score[userIDs[0].m_SteamID] + scoreDiffToWin)
        {
            gameEndMessage.Winner = userIDs[1].m_SteamID;
            MessageManager.SendGameEnd(userIDs[0], gameEndMessage);
            MessageManager.SendGameEnd(userIDs[1], gameEndMessage);
        }
    }
}
