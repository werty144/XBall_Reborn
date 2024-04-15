using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class SteamConnectionCallbacks : MonoBehaviour
{
    public GameStarter GameStarter;
    
    protected Callback<SteamNetConnectionStatusChangedCallback_t> m_ConnectionChanged;

    private void OnEnable()
    {
        if (SteamManager.Initialized)   
        {
            m_ConnectionChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionChanged);
        }
    }

    private void OnConnectionChanged(SteamNetConnectionStatusChangedCallback_t pCallback)
    {
        ConnectionManager connectionManager;
        switch (pCallback.m_info.m_eState)
        {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                if (!GameStarter.Info.IAmMaster) {break;}
                Debug.Log("On connecting");
                StartCoroutine(RelayOnConnectingToConnectionManager(pCallback.m_hConn));
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                Debug.Log("Connection with " + pCallback.m_info.m_identityRemote.GetSteamID().m_SteamID + " established");
                connectionManager = GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>();
                connectionManager.OnConnected(pCallback.m_hConn);
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
                Debug.LogWarning("Problem detected locally: " + pCallback.m_info.m_eEndReason + ". " + pCallback.m_info.m_szEndDebug);
                connectionManager = GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>();
                switch (pCallback.m_info.m_eEndReason)
                {
                    case (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Remote_Timeout:
                        connectionManager.OnRemoteProblem();
                        break;
                    case (int)ESteamNetConnectionEnd.k_ESteamNetConnectionEnd_Misc_Timeout:
                        if (pCallback.m_eOldState ==
                            ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
                        {
                            ((ConnectionManagerFollower)connectionManager).ReconnectTimeOut();
                        }
                        else
                        {
                            connectionManager.OnLocalProblem();
                        }
                        break;
                    default:
                        connectionManager.OnUnknownProblem();
                        Debug.LogWarning("Unknown problem detected locally");
                        break;
                }
                break;
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
                Debug.Log("Closed by peer");
                connectionManager = GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>();
                switch (pCallback.m_eOldState)
                {
                    case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                        connectionManager.OnClosedByPeerWhileConnecting();
                        break;
                    case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                        connectionManager.OnClosedByPeerWhenActive();
                        break;
                    default:
                        Debug.LogWarning("Unsupported connection close");
                        break;
                }
                break;
        }
    }

    IEnumerator RelayOnConnectingToConnectionManager(HSteamNetConnection connection)
    {
        var startTime = DateTime.Now;
        while (true)
        {
            var P2PObject = GameObject.FindWithTag("P2P");
            if (P2PObject != null)
            {
                var ConnectionManager = P2PObject.GetComponent<ConnectionManagerMaster>();
                if (ConnectionManager != null)
                {
                    ConnectionManager.OnConnecting(connection);
                    break;
                }
            }

            if (DateTime.Now - startTime > TimeSpan.FromSeconds(3))
            {
                Debug.LogWarning("Reject connection not in game");
                SteamNetworkingSockets.CloseConnection(connection, 1, "No game is going", false);
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
