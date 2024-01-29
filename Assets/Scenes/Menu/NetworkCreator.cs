using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class NetworkCreator : MonoBehaviour
{
    private void Start()
    {
        if (!SteamManager.Initialized) { return; }
        SteamNetworkingUtils.InitRelayNetworkAccess();
        SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
    }
}
