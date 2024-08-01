using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendItemController : MonoBehaviour
{
    public CSteamID UserID;
    
    public LobbyManager lobbyManager;
    public UIManagerMenu UIManagerMenu;

    public RawImage Avatar;
    public GameObject Nickname;
    public Button InviteButton;
    public Image Tint;
    private Color UnHoverColor = new Color(0, 0, 0, 0);
    private Color HoverColor = new Color(0, 0, 0, 0.4f);

    private void Start()
    {
        StartCoroutine(FetchAvatarUntilSuccess());
        StartCoroutine(FetchNicknameUntilSuccess());
        
        InviteButton.onClick.AddListener(() => {
            lobbyManager.InviteAndCreateOnNeed(UserID);
        });
        InviteButton.onClick.AddListener(() =>
        {
            UIManagerMenu.OnInviteButton();
        });
    }
    
    IEnumerator FetchAvatarUntilSuccess()
    {
        while (true)
        {
            var avatarTexture = Steam.GetUserMediumAvatar(UserID);
            if (avatarTexture != null)
            {
                Avatar.texture = avatarTexture;
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator FetchNicknameUntilSuccess()
    {
        while (true)
        {
            var nickname = Steam.GetUserNickname(UserID);
            if (nickname != null)
            {
                Nickname.GetComponent<TextMeshProUGUI>().text = nickname;
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void OnHover()
    {
        InviteButton.gameObject.SetActive(true);
        Tint.color = HoverColor;
    }

    public void OnUnhover()
    {
        InviteButton.gameObject.SetActive(false);
        Tint.color = UnHoverColor;
    }
}
