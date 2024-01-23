using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Callbacks : MonoBehaviour
{
    public GameObject lobby;
    public GameObject global;
    
    protected Callback<LobbyCreated_t> m_LobbyCreated;
    protected Callback<LobbyEnter_t> m_LobbyEnter;
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    protected Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    protected Callback<SteamNetConnectionStatusChangedCallback_t> m_ConnectionChanged;

    private void OnEnable()
    {
        if (SteamManager.Initialized)   
        {
            m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            m_LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequest);
            m_ConnectionChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionChanged);
        }
    }

    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        Debug.Log("Lobby with id " + pCallback.m_ulSteamIDLobby + " created");
        
        var lobbyManager = lobby.GetComponent<LobbyManager>();
        lobbyManager.OnLobbyCreate(pCallback.m_ulSteamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t pCallback)
    {
        Debug.Log("Enter lobby " + pCallback.m_ulSteamIDLobby);

        var lobbyManager = lobby.GetComponent<LobbyManager>();
        lobbyManager.OnLobbyEnter(pCallback.m_ulSteamIDLobby);
    }
    
    private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
    {
        Debug.Log("Lobby " + pCallback.m_ulSteamIDLobby + " data update.\n" + 
                  "Success: " + pCallback.m_bSuccess + "\n" +
                  "Updated ID: " + pCallback.m_ulSteamIDMember);
        var lobbyManager = lobby.GetComponent<LobbyManager>();
        lobbyManager.OnDataUpdate();
    }
    
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        Debug.Log("Lobby " + pCallback.m_ulSteamIDLobby + "data update." + 
                  "User " + pCallback.m_ulSteamIDUserChanged + "changed status.");
        var lobbyManager = lobby.GetComponent<LobbyManager>();
        lobbyManager.OnDataUpdate();
    }
    
    private void OnGameLobbyJoinRequest(GameLobbyJoinRequested_t pCallback)
    {
        Debug.Log("Got invite into lobby " + pCallback.m_steamIDLobby + " from " + pCallback.m_steamIDFriend);
        SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
    }

    private void OnConnectionChanged(SteamNetConnectionStatusChangedCallback_t pCallback)
    {
        switch (pCallback.m_info.m_eState)
        {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                SteamNetworkingSockets.AcceptConnection(pCallback.m_hConn);
                break;

            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                Debug.Log("Connection with " + pCallback.m_info.m_identityRemote.GetSteamID().m_SteamID + " established");
                var p2pManager = global.GetComponent<P2P>();
                p2pManager.ConnectionEstablished(pCallback.m_hConn);
                break;

            // Handle other states as needed
        }
    }
}
