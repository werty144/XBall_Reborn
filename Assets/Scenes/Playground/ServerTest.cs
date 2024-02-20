using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class ServerTest : Server
{
    // Start is called before the first frame update
    void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManagerMaster>();
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();

        
        var global = GameObject.FindWithTag("Global");
        var gameStarter = global.GetComponent<GameStarter>();
        userIDs[0] = new CSteamID(0);
        userIDs[1] = gameStarter.Info.OpponentID;

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var playerController = player.GetComponent<PlayerController>();
            Players[playerController.ID] = playerController;
        }
        Ball = GameObject.FindWithTag("Ball").GetComponent<BallController>();
    }

    public void RemovePlayers()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            Destroy(player);
        }
        Players.Clear();
    }

    public void GrabPlayers()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var playerController = player.GetComponent<PlayerController>();
            Players[playerController.ID] = playerController;
        }
    }
}
