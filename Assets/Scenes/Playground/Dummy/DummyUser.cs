using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class DummyUser : MonoBehaviour
{
    private TimeSpan ActionFrequency = TimeSpan.FromSeconds(1);
    private DateTime LastAction;
    private ClientDummy Client;
    private void Awake()
    {
        enabled = false;
        Client = GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>();
    }

    void Update()
    {
        if (DateTime.Now - LastAction >= ActionFrequency)
        {
            LastAction = DateTime.Now;
            RandomMove();
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
}
