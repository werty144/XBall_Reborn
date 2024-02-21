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
        enabled = false;
        MessageManager = GameObject.FindWithTag("Dummy").GetComponent<MessageManagerDummy>();
    }
    protected override void Start()
    {
        
    }

    private void OnEnable()
    {
        CreatePlayers();
        CreateBall();
    }

    void CreatePlayers()
    {
        foreach (var player in Players.Values)
        {
            Destroy(player.gameObject);
        }

        var playerPrefab = Resources.Load<GameObject>("Player Variant");
        var layerMask = LayerMask.NameToLayer("Dummy");
        var server = GameObject.FindWithTag("Server").GetComponent<Server>();
        foreach (var serverPlayer in server.GetPlayers().Values)
        {
            var localPlayer = Instantiate(playerPrefab, serverPlayer.transform.position, serverPlayer.transform.rotation);
            localPlayer.layer = layerMask;
            var playerController = localPlayer.GetComponent<PlayerController>();
            playerController.ID = serverPlayer.ID;
            Players[playerController.ID] = playerController;
            if (serverPlayer.ID % 2 == 1)
            {
                playerController.IsMy = true;
                MyPlayers.Add(playerController);
            }
        }
        
        foreach (var playerController in Players.Values)
        {
            foreach (Renderer renderer in playerController.GetComponentsInChildren<Renderer>())
            {
                // renderer.enabled = true;
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color;
                        if (playerController.ID % 2 == 0)
                        {
                            color = PlayerConfig.MyColor;
                        }
                        else
                        {
                            color = PlayerConfig.OpponentColor;
                        }

                        color.a = 0.5f;
                        material.color = color;
                    }
                }
            }
        }
    }
    
    new void CreateBall()
    {
        Destroy(Ball);
        var server = GameObject.FindWithTag("Server").GetComponent<Server>();
        var serverBall = server.GetBall();
        var ballPrefab = Resources.Load<GameObject>("Ball Variant");
        var ballObject = Instantiate(ballPrefab,serverBall.transform.position, server.transform.rotation);
        
        ballObject.GetComponentInChildren<Renderer>().enabled = true;
        
        int collisionLayer = LayerMask.NameToLayer("Dummy");
        ballObject.layer = collisionLayer;
        
        Ball = ballObject.GetComponent<BallController>();
    }

    public List<PlayerController> GetMyPlayers()
    {
        return MyPlayers;
    }
}
