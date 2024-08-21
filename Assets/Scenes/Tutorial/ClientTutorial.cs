using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Steamworks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ClientTutorial : Client
{
    protected override void Awake()
    {
        var setupInfo = new SetupInfo
        {
            IAmMaster =  true,
            NumberOfPlayers = 0,
            Speed = LobbyManager.SpeedNormal,
            OpponentID = new CSteamID(1),
            MyID = Steam.MySteamID(),
            LobbyID = new CSteamID(2)
        };
        InitiateGoals(setupInfo);
        
        int collisionLayer = LayerMask.NameToLayer("Client");
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
        ballObject.SetActive(false);

        var initPlayer = AddMyPlayer(0, 0, new PlayerState());
        initPlayer.SetActive(false);
    }

    protected override void Start()
    {
        
    }

    protected override void FixedUpdate()
    {
        
    }

    public GameObject AddMyPlayer(uint playerID, int playerNumber, PlayerState playerState)
    {
        var player = Instantiate(ClientPlayerPrefabBlue);
        player.layer = LayerMask.NameToLayer("Client");;
        var controller = player.GetComponent<PlayerController>();
        controller.ID = playerID;
        controller.IsMy = true;
        controller.Ball = Ball;
        controller.UserID = Steam.MySteamID().m_SteamID;
        Players[playerID] = controller;
        player.GetComponent<SelectionManager>().PlayerNumber = playerNumber;
        controller.ApplyState(playerState);
        return player;
    }
    
    public override void InputAction(IBufferMessage action)
    {
        PlayerController player;
        switch (action)
        {
            case PlayerMovementAction playerMovementAction:
                player = Players[playerMovementAction.PlayerId];
                var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
                player.SetMovementTarget(target);
                break;
            case PlayerStopAction playerStopAction:
                player = Players[playerStopAction.PlayerId];
                player.Stop();
                break;
            case GrabAction grabAction:
                player = Players[grabAction.PlayerId];
                player.PlayGrabAnimation();
                grabAction.PreSuccess = ActionRules.BallGrabSuccess(player, Ball);
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
                var goalSuccess = GoalRules.GoalAttemptSuccess(Players, Ball.Owner, Ball, Goals[OpponentID]);
                throwAction.GoalSuccess = goalSuccess;
                break;
            default:
                Debug.LogWarning("Unknown input action");
                break;
        }
    }
}
