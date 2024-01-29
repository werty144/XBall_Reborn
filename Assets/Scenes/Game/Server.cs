using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using UnityEngine;

public class Server : MonoBehaviour, StateHolder
{
    private P2PMaster P2PMaster;
    private CSteamID[] userIDs = new CSteamID[2];
    private HashSet<CSteamID> PeersReady = new HashSet<CSteamID>();
    
    private Dictionary<uint, PlayerController> Players = new ();
    private GameStateVersioning GameStateVersioning;

    private void Awake()
    {
        GameStateVersioning = new GameStateVersioning(this);
    }

    private void Start()
    {
        P2PMaster = GameObject.FindWithTag("P2P").GetComponent<P2PMaster>();
        
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
        GameStateVersioning.AddCurrentState();
    }

    public void ProcessAction(CSteamID actorID, IBufferMessage action)
    {
        var actorPing = P2PMaster.GetPingToUser(actorID);
        GameStateVersioning.ApplyActionInThePast(action, actorID, actorPing);

        var currentState = GetGameState();
        foreach (var userID in userIDs)
        {
            var userPing = P2PMaster.GetPingToUser(userID);
            GameStateVersioning.FastForward(userPing);
            var stateForUser = GetGameState();
            if (userID == actorID)
            {
                P2PMaster.SendActionResponse(
                    actorID,
                    new ActionResponse
                    {
                        ActionId = ParseUtils.GetActionId(action),
                        GameState = stateForUser
                    });
            }
            else
            {
                P2PMaster.SendGameState(userID, stateForUser);
            }

            ApplyGameState(currentState);
        }
    }

    public void PeerReady(CSteamID userID)
    {
        PeersReady.Add(userID);

        if (PeersReady.Count == 2)
        {
            foreach (var user in userIDs)
            {
                P2PMaster.SendGameStart(user);
            }
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
}
