using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class DummyPlayer : MonoBehaviour
{
    public GameObject slider;

    private GameStarter gameStarter;
    private MessageManagerTest messageManager;
    
    private List<PlayerController> playersToControll;
    private TimeSpan ActionFrequency = TimeSpan.FromSeconds(1);
    private DateTime LastAction;

    private TimeSpan Ping = TimeSpan.Zero;

    private void Awake()
    {
        enabled = false;
        messageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManagerTest>();
    }

    void OnEnable()
    {
        GrabPlayers();
        messageManager.DummyReady();
    }

    public void Disable()
    {
        enabled = false;
    }

    public void Enable()
    {
        enabled = true;
    }

    void GrabPlayers()
    {
        playersToControll = new List<PlayerController>();
        LastAction = DateTime.Now;
        
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var controller = player.GetComponent<PlayerController>();
            if (!controller.IsMy)
            {
                playersToControll.Add(controller);
            }
        }
    }

    void Update()
    {
        if (DateTime.Now - LastAction >= ActionFrequency)
        {
            LastAction = DateTime.Now;
            RandomMove();
        }
    }

    void Static()
    {
        var playerIndex = 0;
        var action = new PlayerMovementAction
        {
            PlayerId = playersToControll[playerIndex].ID,
            X = playersToControll[playerIndex].GetState().X,
            Y = playersToControll[playerIndex].GetState().Y
        };
        messageManager.DummySendAction(action);
    }

    void RandomMove()
    {
        var random = new Random();
        var playerIndex = random.Next(0, playersToControll.Count);
        var playerID = playersToControll[playerIndex].ID;  
        var randX = random.Next(0, 20) - 10;
        var randY = random.Next(0, 30) - 15;
        var action = new PlayerMovementAction
        {
            PlayerId = playerID,
            X = randX,
            Y = randY
        };
        messageManager.DummySendAction(action);
    }
}
