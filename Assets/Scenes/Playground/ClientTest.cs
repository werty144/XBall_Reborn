using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientTest : Client
{
    // Start is called before the first frame update

    void Awake()
    {
        
    }
    void Start()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;
        
        CreatePlayers(setupInfo.NumberOfPlayers, setupInfo.IAmMaster);
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
        
        
        GameStateVersioning = new GameStateVersioning(this);
    }

    public void CreatePlayers(int n)
    {
        CreatePlayers(n, true);
    }
}
