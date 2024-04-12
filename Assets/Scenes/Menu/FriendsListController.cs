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
    public GameObject lobby;
    public GameObject UIManagerMenu;
    void Start()
    {
        UpdateFriendsList();
        GameObject.FindGameObjectWithTag("Global").GetComponent<Callbacks>().SetFriendsList(this);
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
            controller.lobby = lobby;
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
}
