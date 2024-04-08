using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerMenu : MonoBehaviour
{
    public GameObject FriendsOnlineView;
    public GameObject LobbyView;
    public GameObject InitialView;
    private void Start()
    {

    }

    public void OnPlayFriendButton()
    {
        InitialView.SetActive(false);
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
        FriendsOnlineView.SetActive(false);
        LobbyView.SetActive(true);
    }
}
