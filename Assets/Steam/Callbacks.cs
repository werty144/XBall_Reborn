using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Callbacks : MonoBehaviour
{
    protected Callback<LobbyCreated_t> m_LobbyCreated;
    protected Callback<LobbyEnter_t> m_LobbyEnter;
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    protected Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

    private void OnEnable()
    {
        if (SteamManager.Initialized)   
        {
            m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        }
    }

    private void OnLobbyCreated(LobbyCreated_t pCallback)
    {
        print("Lobby with id " + pCallback.m_ulSteamIDLobby + " created");
    }
}
