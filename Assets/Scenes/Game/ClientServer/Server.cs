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
        PlayerPrefab = Resources.Load<GameObject>("Player Variant");
        BallPrefab = Resources.Load<GameObject>("Ball Variant");
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

        CreatePlayers(gameStarter.Info.NumberOfPlayers);
        CreateBall();
    }
    
    private void CreatePlayers(int n)
    {
        var defaultPlaneWidth = 10;
        var defaultPlaneLength = 10;
        
        var floor = GameObject.FindWithTag("Floor");
        var scale = floor.GetComponent<Transform>().localScale;
        var fieldWidth = scale.x * defaultPlaneWidth;
        var fieldLength = scale.z * defaultPlaneLength;
        
        byte spareID = 0;
        float masterZ = -fieldLength / 4;
        float followerZ = fieldLength / 4;
        int collisionLayer = LayerMask.NameToLayer("Server");
        for (int i = 0; i < n; i++)
        {
            var x = fieldWidth * (i + 1) / (n + 1) - fieldWidth / 2;
            
            var masterPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, masterZ), Quaternion.identity);
            masterPlayer.layer = collisionLayer;
            var controller = masterPlayer.GetComponent<PlayerController>();
            controller.ID = spareID;
            Players[spareID] = controller;
            spareID++;
            
            var followerPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.Euler(0, 180, 0));
            followerPlayer.layer = collisionLayer;
            var followerContorller = followerPlayer.GetComponent<PlayerController>();
            followerContorller.ID = spareID;
            Players[spareID] = followerContorller;
            spareID++;
        }

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

    private void CreateBall()
    {
        int collisionLayer = LayerMask.NameToLayer("Server");
        var ballObject = Instantiate(BallPrefab, new Vector3(0, GameConfig.SphereRadius, 0), Quaternion.identity);
        ballObject.GetComponentInChildren<Renderer>().enabled = false;
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
    }

    private void Update()
    {
        if (OnPause) {return;}
        GameStateVersioning.AddCurrentState();
    }

    public void ProcessAction(CSteamID actorID, IBufferMessage action)
    {
        if (OnPause) {return;}

        if (IsConflictingAction(action))
        {
            switch (action)
            {
                case GrabAction grabAction:
                    GameStateVersioning.ApplyActionToCurrentState(grabAction);
                    break;
                default:
                    Debug.LogWarning("Unknown action");
                    break;
            }
        }
        else
        {
            GameStateVersioning.ApplyActionToCurrentState(action);
            MessageManager.SendGameState(GetAnotherID(actorID), GetGameState());
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

    private bool IsConflictingAction(IBufferMessage action)
    {
        switch (action)
        {
            case PlayerMovementAction:
                return false;
            case PlayerStopAction:
                return false;
            case GrabAction:
                return true;
            default:
                Debug.LogWarning("Unknown action");
                return true;
        }
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
}
