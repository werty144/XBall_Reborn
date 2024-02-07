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
                
        var connectionParams = new SteamNetworkingConfigValue_t[1];
        connectionParams[0].m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected;
        connectionParams[0].m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
        connectionParams[0].m_val = new SteamNetworkingConfigValue_t.OptionValue{ m_int32 = 1000 };
        SteamNetworkingSockets.CreateListenSocketP2P(0, connectionParams.Length, connectionParams);
    }
}
