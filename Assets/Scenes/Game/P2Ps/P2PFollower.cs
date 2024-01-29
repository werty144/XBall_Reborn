using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class P2PFollower : P2PBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnected(HSteamNetConnection connection)
    {
        Client.OnConnected();
    }
}
