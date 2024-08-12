using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpponentPlateManager : MonoBehaviour
{
    public CSteamID opponentID;
    public RawImage opponentAvatar;
    public TextMeshProUGUI opponentNick;
    private void OnEnable()
    {
        StartCoroutine(FetchAvatarUntilSuccess());
        StartCoroutine(FetchNicknameUntilSuccess());
    }
    
    IEnumerator FetchAvatarUntilSuccess()
    {
        while (true)
        {
            var opponentAvatarTexture = Steam.GetUserLargeAvatar(opponentID);
            if (opponentAvatarTexture != null)
            {
                opponentAvatar.texture = opponentAvatarTexture;
                break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    IEnumerator FetchNicknameUntilSuccess()
    {
        while (true)
        {
            var opponentNickname = Steam.GetUserNickname(opponentID);
            if (opponentNickname != null)
            {
                opponentNick.text = opponentNickname;
                break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
