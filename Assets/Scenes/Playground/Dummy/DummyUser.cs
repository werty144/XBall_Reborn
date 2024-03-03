using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

enum PlayerIntention
{
    None = 0,
    Grab = 1,
    Follow = 2
}

public class DummyUser : MonoBehaviour
{
    private TimeSpan ActionFrequency = TimeSpan.FromSeconds(1);
    private DateTime LastAction;
    private ClientDummy Client;
    private Dictionary<uint, PlayerIntention> PlayerIntentions = new();
    private void Awake()
    {
        Client = GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>();

        foreach (var player in Client.GetMyPlayers())
        {
            PlayerIntentions[player.ID] = PlayerIntention.None;
        }
    }

    void Update()
    {
        foreach (var (playerID, intention) in PlayerIntentions)
        {
            var player = Client.GetPlayers()[playerID];
            switch (intention)
            {
                case PlayerIntention.None:
                    break;
                case PlayerIntention.Grab:
                    var ball = Client.GetBall();
                    if (ball.Owned && ball.Owner.ID == playerID)
                    {
                        PlayerIntentions[playerID] = PlayerIntention.None;
                        return;
                    }
                    if (ActionRules.IsValidGrab(player.transform, Client.GetBall().transform))
                    {
                        var grabAction = new GrabAction
                        {
                            PlayerId = playerID
                        };
                        Client.InputAction(grabAction);
                        return;
                    }

                    var movementAction = new PlayerMovementAction
                    {
                        PlayerId = playerID,
                        X = ball.transform.position.x,
                        Y = ball.transform.position.z
                    };
                    Client.InputAction(movementAction);
                    break;
                case PlayerIntention.Follow:
                    break;
            }
        }
    }

    void RandomMove()
    {
        var random = new Random();
        var playerIndex = random.Next(0, Client.GetMyPlayers().Count);
        var player = Client.GetMyPlayers()[playerIndex];
        var playerID = player.ID;  
        var randX = random.Next(0, 20) - 10;
        var randY = random.Next(0, 30) - 15;
        var action = new PlayerMovementAction
        {
            PlayerId = playerID,
            X = randX,
            Y = randY
        };
        player.SetMovementTarget(new Vector2(randX, randY));
        Client.InputAction(action);
    }

    public void SetPlayerGrabIntention(uint playerID)
    {
        PlayerIntentions[playerID] = PlayerIntention.Grab;
    }
}
