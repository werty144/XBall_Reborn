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
    public GameObject BallPrefab;

    protected MessageManager MessageManager;
    
    private Dictionary<uint, PlayerController> Players = new();
    private BallController Ball;
    protected GameStateVersioning GameStateVersioning;
    private uint NextActionId = 1;

    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;
        
        CreatePlayers(setupInfo.NumberOfPlayers, setupInfo.IAmMaster);
        CreateBall();
        
        GameStateVersioning = new GameStateVersioning(this);
    }

    private void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
    }

    private void CreatePlayers(int n, bool IAmMaster)
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
        int collisionLayer = LayerMask.NameToLayer("Client");
        for (int i = 0; i < n; i++)
        {
            var x = fieldWidth * (i + 1) / (n + 1) - fieldWidth / 2;
            
            var masterPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, masterZ), Quaternion.identity);
            masterPlayer.layer = collisionLayer;
            var controller = masterPlayer.GetComponent<PlayerController>();
            controller.IsMy = IAmMaster;
            controller.ID = spareID;
            Players[spareID] = controller;
            spareID++;
            controller.Colorize(PlayerConfig.MyColor);
            
            var followerPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.Euler(0, 180, 0));
            followerPlayer.layer = collisionLayer;
            var followerContorller = followerPlayer.GetComponent<PlayerController>();
            followerContorller.IsMy = !IAmMaster;
            followerContorller.ID = spareID;
            Players[spareID] = followerContorller;
            spareID++;
            followerContorller.Colorize(PlayerConfig.OpponentColor);
        }
    }

    protected void CreateBall()
    {
        int collisionLayer = LayerMask.NameToLayer("Client");
        var ballObject = Instantiate(BallPrefab, new Vector3(0, GameConfig.SphereRadius, 0), Quaternion.identity);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
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
                player.SetMovementTarget(target);

                playerMovementAction.ActionId = NextActionId;
                NextActionId++;
                break;
            case PlayerStopAction playerStopAction:
                player = Players[playerStopAction.PlayerId];
                //Invalid action
                if (!player.IsMy) { return; }

                player.Stop();
                playerStopAction.ActionId = NextActionId;
                NextActionId++;
                break;
            case GrabAction grabAction:
                player = Players[grabAction.PlayerId];
                //Invalid action
                if (!player.IsMy) { return; }
                player.PlayGrabAnimation();
                if (!ActionRules.IsValidGrab(player.transform, Ball.transform))
                {
                    return;
                }
                grabAction.ActionId = NextActionId;
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
                player.SetMovementTarget(new Vector2(playerState.TargetX, playerState.TargetY));
            }
            else
            {
                var currentPosition = player.GetPosition();
                var referencePosition = new Vector2(playerState.X, playerState.Y);
                if (Vector2.Distance(currentPosition, referencePosition) >= PlayerConfig.Radius)
                {
                    player.SetMovementTarget(referencePosition);
                }

                if (Mathf.Abs(player.GetAngle() - playerState.RotationAngle) >= 0.1)
                {
                    player.SetRotationTargetAngle(playerState.RotationAngle);
                }
            }
        }
    }

    public void ReceiveActionResponse(ActionResponse actionResponse)
    {
        // if (!PlayerToLastAction.ContainsValue(actionResponse.ActionId))
        // {
        //     return;
        // }
        // ReceiveState(actionResponse.GameState);
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
}
