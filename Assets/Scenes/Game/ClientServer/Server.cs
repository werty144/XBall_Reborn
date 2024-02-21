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
    private GameStateVersioning GameStateVersioning;
    
    private GameState PausedState;
    private bool OnPause;
    
    private void Awake()
    {
        PlayerPrefab = Resources.Load<GameObject>("ServerPlayerPrefab");
        BallPrefab = Resources.Load<GameObject>("ServerBallPrefab");
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
    }
    
    private void CreateInitialState(int n)
    {
        var collisionLayer = LayerMask.NameToLayer("Server");
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var player = Instantiate(PlayerPrefab);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            Players[spareID] = controller;
            spareID++;
            foreach (Renderer renderer in controller.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
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
                GameStateVersioning.ApplyActionToCurrentState(grabAction);
                var relayedAction = new RelayedAction
                {
                    UserId = actorID.m_SteamID,
                    GrabAction = grabAction,
                    Success = true
                };
                foreach (var userID in userIDs)
                {
                    MessageManager.RelayAction(userID, relayedAction);
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
        var curGameState = GetGameState();
        foreach (var userID in userIDs)
        {
            MessageManager.SendGameState(userID, curGameState);
        }
    }
}
