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
    private P2PTestMaster P2PManager;
    
    private List<PlayerController> playersToControll;
    private TimeSpan ActionFrequency = TimeSpan.FromSeconds(1);
    private DateTime LastAction;

    private TimeSpan RTT = TimeSpan.FromMilliseconds(200);

    private void Awake()
    {
        gameStarter = GameObject.FindWithTag("Global").GetComponent<GameStarter>();
        if (!gameStarter.IsTest)
        {
            enabled = false;
        }
        
        P2PManager = GameObject.FindWithTag("P2P").GetComponent<P2PTestMaster>();
    }

    void Start()
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
        
        P2PManager.DummyReady();
    }

    // Update is called once per frame
    void Update()
    {
        // GameManager.LastRTT(RTT);
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
            Id = playersToControll[playerIndex].ID,
            X = playersToControll[playerIndex].GetState().X,
            Y = playersToControll[playerIndex].GetState().Y
        };
        // GameManager.GetComponent<GameManager>().OpponentAction(action);
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
            Id = playerID,
            X = randX,
            Y = randY
        };
        // GameManager.GetComponent<GameManager>().OpponentAction(action);
    }

    public void OnSliderChange()
    {
        RTT = TimeSpan.FromMilliseconds((int) (slider.GetComponent<Slider>().value * 2000f));
    }
}
