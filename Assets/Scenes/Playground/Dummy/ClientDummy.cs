using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientDummy : Client
{
    private List<PlayerController> MyPlayers = new();

    protected override void Awake()
    {
        MyID = 1;
        MessageManager = GameObject.FindWithTag("Dummy").GetComponent<MessageManagerDummy>();
        
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;
        CreateServerState(setupInfo.NumberOfPlayers, LayerMask.NameToLayer("DummyServer"));
        CreateInitialState(setupInfo.NumberOfPlayers);
        InitiateCooldowns();
        Hide();
    }

    protected override void Start()
    {
        
    }

    public void Show()
    {
        foreach (var playerController in Players.Values)
        {
            foreach (Renderer renderer in playerController.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }
        foreach (Renderer renderer in Ball.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
    }

    public void Hide()
    {
        foreach (var playerController in Players.Values)
        {
            if (playerController == null)
            {
                continue;
            }
            foreach (Renderer renderer in playerController.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }

        if (Ball == null)
        {
            return;
        }
        foreach (Renderer renderer in Ball.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }

    void CreateInitialState(int n)
    {
        int collisionLayer = LayerMask.NameToLayer("Dummy");
        
        var ballObject = Instantiate(BallPrefab);
        ballObject.layer = collisionLayer;
        Ball = ballObject.GetComponent<BallController>();
        
        uint spareID = 0;
        for (int i = 0; i < 2 * n; i++)
        {
            var player = Instantiate(i % 2 == 0 ? ClientPlayerPrefabBlue : ClientPlayerPrefabRed);
            player.layer = collisionLayer;
            var controller = player.GetComponent<PlayerController>();
            controller.ID = spareID;
            controller.Ball = Ball;
            Players[spareID] = controller;
            spareID++;
        }

        foreach (var player in Players.Values)
        {
            player.IsMy = player.ID % 2 == 1;
            if (player.IsMy)
            {
                MyPlayers.Add(player);
            }
        }

        ApplyGameState(InitialState.GetInitialState(n));
        
        foreach (var playerController in Players.Values)
        {
            foreach (Renderer renderer in playerController.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 0.5f;
                        material.color = color;
                    }
                }
            }
        }
    }

    public List<PlayerController> GetMyPlayers()
    {
        return MyPlayers;
    }

    public override void ReceiveGoalAttempt(GoalAttempt goalAttempt)
    {
    }
}
