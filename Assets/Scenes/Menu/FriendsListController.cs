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
            var nicknamePlate = newItem.GetComponentInChildren<TextMeshProUGUI>();
            if (nicknamePlate != null)
            {
                nicknamePlate.text = friend.Name;
            }
            else
            {
                Debug.LogWarning("Nickname plate not found");
            }

            var inviteButton = newItem.GetComponentInChildren<Button>();
            if (inviteButton != null)
            {
                inviteButton.onClick.AddListener(() => {
                    var lobbyManager = lobby.GetComponent<LobbyManager>();
                    lobbyManager.InviteAndCreateOnNeed(friend.ID);
                });
            }
            else
            {
                Debug.LogWarning("Invite button not found");
            }
        }
    }

    void ClearFriendsList()
    {
        for (int i = contentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(contentPanel.GetChild(i).gameObject);
        }
    }
}
