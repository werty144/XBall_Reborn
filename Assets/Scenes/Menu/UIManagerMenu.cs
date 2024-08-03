using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerMenu : MonoBehaviour
{
    public GameObject FriendsOnlineView;
    public GameObject LobbyView;
    public GameObject InitialView;

    public ParticleSystem smoke;

    public void OnPlayFriendButton()
    {
        InitialView.SetActive(false);
        smoke.Stop();
        FriendsOnlineView.SetActive(true);
    }

    public void OnInviteButton()
    {
        FriendsOnlineView.SetActive(false);
        LobbyView.SetActive(true);
    }

    public void OnLobbyEnter()
    {
        InitialView.SetActive(false);
        smoke.Stop();
        FriendsOnlineView.SetActive(false);
        LobbyView.SetActive(true);
    }

    public void OnLobbyLeave()
    {
        LobbyView.SetActive(false);
        InitialView.SetActive(true);
        smoke.Play();
    }

    public void OnCloseFriendsList()
    {
        InitialView.SetActive(true);
        smoke.Play();
        FriendsOnlineView.SetActive(false);
    }
}
