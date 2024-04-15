using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class SteamLobbyCallbacks : MonoBehaviour
{
    public LobbyManager lobby;
    public UIManagerMenu UIManagerMenu;
    
    protected Callback<LobbyCreated_t> m_LobbyCreated;
    protected Callback<LobbyEnter_t> m_LobbyEnter;
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    protected Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    protected Callback<SteamNetConnectionStatusChangedCallback_t> m_ConnectionChanged;
    protected Callback<PersonaStateChange_t> m_PersonaStateChange;

    private void OnEnable()
    {
        if (SteamManager.Initialized)   
        {
            m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            m_LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequest);
        }
    }

    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        lobby.OnLobbyCreate(pCallback.m_ulSteamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t pCallback)
    {
        UIManagerMenu.OnLobbyEnter();
        lobby.OnLobbyEnter(pCallback.m_ulSteamIDLobby);
    }
    
    private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
    {
        // We receive member data update twice: as a member update and as a lobby update
        // Hence, we can only handle lobby updates
        if (pCallback.m_ulSteamIDMember != pCallback.m_ulSteamIDLobby) { return; }
        
        lobby.OnDataUpdate();
    }
    
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        // This one is handled as a part of LobbyDataUpdate
    }
    
    private void OnGameLobbyJoinRequest(GameLobbyJoinRequested_t pCallback)
    {
        SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
    }

    public void OnDestroy()
    {
        m_LobbyCreated.Dispose();
        m_LobbyEnter.Dispose();
        m_LobbyDataUpdate.Dispose();
        m_LobbyChatUpdate.Dispose();
        m_GameLobbyJoinRequested.Dispose();
    }
}
