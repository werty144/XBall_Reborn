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
    void Start()
    {
        StartCoroutine(UpdateFriendsListCoroutine(1f));
    }
    
    IEnumerator UpdateFriendsListCoroutine(float updateInterval)
    {
        while (true)
        {
            UpdateFriendsList();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void UpdateFriendsList()
    {
        ClearFriendsList();
        // SteamUser[] onlineFriends =
        // {
        //     new SteamUser(new CSteamID(1), "Anton"),
        //     new SteamUser(new CSteamID(2), "Grisha")
        // };
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
                    Steam.SendInvite(friend.ID);
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
