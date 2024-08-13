using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListController : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform contentPanel;
    public LobbyManager lobbyManager;
    public UIManagerMenu UIManagerMenu;
    
    protected Callback<PersonaStateChange_t> m_PersonaStateChange;
    void Start()
    {
        UpdateFriendsList();
        m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
    }

    private void OnPersonaStateChange(PersonaStateChange_t pCallback)
    {
        UpdateFriendsList();
    }

    public void UpdateFriendsList()
    {
        ClearFriendsList();
        var onlineFriends = Steam.GetOnlineFriends();

        foreach (var friend in onlineFriends)
        {
            GameObject newItem = Instantiate(listItemPrefab, contentPanel);
            var controller = newItem.GetComponent<FriendItemController>();
            controller.UserID = friend.ID;
            controller.lobbyManager = lobbyManager;
            controller.UIManagerMenu = UIManagerMenu;
        }
    }

    void ClearFriendsList()
    {
        for (int i = contentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(contentPanel.GetChild(i).gameObject);
        }
    }

    public void Close()
    {
        UIManagerMenu.GetComponent<UIManagerMenu>().OnCloseFriendsList();
    }

    private void OnDestroy()
    {
        m_PersonaStateChange.Dispose();
    }
}
