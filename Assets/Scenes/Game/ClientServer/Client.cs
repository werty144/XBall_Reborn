using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;

public class Client : MonoBehaviour, StateHolder
{
    public GameObject PlayerPrefab;

    protected MessageManager MessageManager;
    
    private Dictionary<uint, PlayerController> Players = new();
    protected GameStateVersioning GameStateVersioning;
    private Dictionary<uint, uint> PlayerToLastAction = new();
    private uint NextActionId = 1;

    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;
        
        CreatePlayers(setupInfo.NumberOfPlayers, setupInfo.IAmMaster);
        
        GameStateVersioning = new GameStateVersioning(this);
    }

    private void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
    }

    protected void CreatePlayers(int n, bool IAmMaster)
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
        for (int i = 0; i < n; i++)
        {
            var x = fieldWidth * (i + 1) / (n + 1) - fieldWidth / 2;
            
            var masterPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, masterZ), Quaternion.identity);
            var controller = masterPlayer.GetComponent<PlayerController>();
            controller.Initialize(IAmMaster, spareID);
            Players[spareID] = controller;
            spareID++;
            
            var followerPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.identity);
            var followerContorller = followerPlayer.GetComponent<PlayerController>();
            followerContorller.Initialize(!IAmMaster, spareID);
            Players[spareID] = followerContorller;
            spareID++;
        }
    }

    public void InputAction(IBufferMessage action)
    {
        PlayerController player;
        // Optimistic execution
        switch (action)
        {
            case PlayerMovementAction playerMovementAction:
                player = Players[playerMovementAction.PlayerId];
                //Invalid action
                if (!player.IsMy) { return; }
                
                var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
                player.SetTarget(target);

                PlayerToLastAction[playerMovementAction.PlayerId] = NextActionId;
                playerMovementAction.ActionId = NextActionId;
                NextActionId++;
                break;
            case PlayerStopAction playerStopAction:
                player = Players[playerStopAction.PlayerId];
                //Invalid action
                if (!player.IsMy) { return; }

                player.Stop();
                PlayerToLastAction[playerStopAction.PlayerId] = NextActionId;
                playerStopAction.ActionId = NextActionId;
                NextActionId++;
                break;
            default:
                Debug.LogWarning("Unknown input action");
                break;
        }
        
        MessageManager.SendAction(action);
    }

    public void ReceiveState(GameState gameState)
    {
        CorrectOpponentPlayers(gameState);
    }

    private void CorrectOpponentPlayers(GameState referenceState)
    {
        foreach (var playerState in referenceState.PlayerStates)
        {
            var player = GetPlayers()[playerState.Id];
            if (player.IsMy) {continue;}

            if (playerState.IsMoving)
            {
                player.SetTarget(new Vector2(playerState.TargetX, playerState.TargetY));
            }
            else
            {
                var currentPosition = player.GetPosition();
                var referencePosition = new Vector2(playerState.X, playerState.Y);
                if (Vector2.Distance(currentPosition, referencePosition) >= PlayerConfig.Radius)
                {
                    player.SetTarget(referencePosition);
                }
            }
        }
    }

    public void ReceiveActionResponse(ActionResponse actionResponse)
    {
        if (!PlayerToLastAction.ContainsValue(actionResponse.ActionId))
        {
            return;
        }
        ReceiveState(actionResponse.GameState);
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
