using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Steamworks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ClientTutorial : Client
{
    public GameObject BallInstance;
    public GameObject MyPlayer1;
    public GameObject MyPlayer2;
    public GameObject OpponentPlayer;

    public bool scored;

    private Pig lastThrower;
    private bool lastThrowSuccess;
    private int stealAttempts;
    protected override void Awake()
    {
        OpponentID = 1;
        MyID = Steam.MySteamID().m_SteamID;
        var setupInfo = new SetupInfo
        {
            IAmMaster =  true,
            NumberOfPlayers = 0,
            Speed = LobbyManager.SpeedNormal,
            OpponentID = new CSteamID(OpponentID),
            MyID = new CSteamID(MyID),
            LobbyID = new CSteamID(228)
        };
        InitiateGoals(setupInfo);
        
        Ball = BallInstance.GetComponent<BallController>();
        AddPlayer(MyPlayer1);
        AddPlayer(MyPlayer2);
        AddPlayer(OpponentPlayer);
    }

    protected override void Start()
    {
        
    }

    protected override void FixedUpdate()
    {
        
    }

    void AddPlayer(GameObject player)
    {
        var controller = player.GetComponent<PlayerController>();
        controller.Ball = Ball;
        controller.UserID = controller.IsMy ? Steam.MySteamID().m_SteamID : 1;
        Players[controller.ID] = controller;
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
                if (player.IsMy && Ball.Owned && !Ball.Owner.IsMy)
                {
                    stealAttempts++;
                    if (stealAttempts == 1)
                    {
                        break;
                    }
                }
                StartCoroutine(DelayedAction(() =>
                {
                    Ball.SetOwner(player);
                }, ActionRulesConfig.GrabDuration));
                break;
            case ThrowAction throwAction:
                if (!Ball.Owned || !Ball.Owner.IsMy)
                {
                    return;
                }
                var initialTarget = ProtobufUtils.FromVector3Protobuf(throwAction.Destination);
                var resultingTarget = ActionRules.CalculateThrowTarget(Ball.Owner, initialTarget);
                throwAction.Destination = ProtobufUtils.ToVector3ProtoBuf(resultingTarget);
                lastThrowSuccess = GoalRules.GoalAttemptSuccess(Players, Ball.Owner, Ball, Goals[OpponentID]);
                lastThrower = Ball.Owner.gameObject.GetComponent<Pig>();
                Ball.Owner.PlayThroughAnimation();
                StartCoroutine(DelayedAction(() =>
                {
                    Ball.Owner.GetComponent<GrabManager>().SetCooldownMillis(2000f / Time.timeScale);
                    Ball.ThrowTo(resultingTarget);
                }, ActionRulesConfig.ThrowDuration));
                break;
            default:
                Debug.LogWarning("Unknown input action");
                break;
        }
    }
    
    IEnumerator DelayedAction(Action action, int delayMillis)
    {
        yield return new WaitForSeconds(0.001f * delayMillis);
        action();
    }

    public override void ReceiveGoalAttempt(GoalAttempt goalAttempt)
    {
        Goals[1].GetComponentInChildren<Animator>().Play(lastThrowSuccess ? "success" : "failure", -1, 0f);
        if (lastThrowSuccess)
        {
            scored = true;
            lastThrower.Piggiwise();
            Goals[1].GetComponentInChildren<Animator>().Play("success", -1, 0f);
        }
        else
        {
            Goals[1].GetComponentInChildren<Animator>().Play("failure", -1, 0f);
        }
    }
}
