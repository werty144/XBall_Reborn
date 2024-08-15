using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerMenu : MonoBehaviour
{
    public GameObject FriendsOnlineView;
    public GameObject LobbyView;
    public GameObject InitialView;
    public GameObject ExitView;

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

    public void OnToExitButton()
    {
        InitialView.SetActive(false);
        smoke.Stop();
        ExitView.SetActive(true);
    }

    public void OnReturnFromExitScreen()
    {
        ExitView.SetActive(false);
        InitialView.SetActive(true);
        smoke.Play();
    }

    public void OnTutorial()
    {
        GameObject.FindWithTag("SceneTransition").GetComponent<SceneTransition>().LoadScene("Tutorial");
    }
}
