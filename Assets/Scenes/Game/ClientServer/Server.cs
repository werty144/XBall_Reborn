using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class Server : MonoBehaviour, StateHolder
{
    protected MessageManagerMaster MessageManager;
    protected PingManager PingManager;
    protected CSteamID[] userIDs = new CSteamID[2];
    private HashSet<CSteamID> PeersReady = new HashSet<CSteamID>();
    
    protected Dictionary<uint, PlayerController> Players = new ();
    private GameStateVersioning GameStateVersioning;
    
    private GameState PausedState;
    private bool OnPause;
    
    private void Awake()
    {
        GameStateVersioning = new GameStateVersioning(this);
    }

    private void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManagerMaster>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
        
        var global = GameObject.FindWithTag("Global");
        var gameStarter = global.GetComponent<GameStarter>();
        userIDs[0] = Steam.MySteamID();
        userIDs[1] = gameStarter.Info.OpponentID;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var playerController = player.GetComponent<PlayerController>();
            Players[playerController.ID] = playerController;
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

        if (IsConflictingAction(action))
        {
            
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
