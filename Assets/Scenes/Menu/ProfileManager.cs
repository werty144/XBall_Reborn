using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public RawImage Avatar;
    public GameObject Nickname;
    public TextMeshProUGUI Winrate;
    void OnEnable()
    {
        StartCoroutine(FetchAvatarUntilSuccess());
        StartCoroutine(FetchNicknameUntilSuccess());
        FetchWinrate();
    }

    IEnumerator FetchAvatarUntilSuccess()
    {
        while (true)
        {
            var avatarTexture = Steam.GetUserLargeAvatar(Steam.MySteamID());
            if (avatarTexture != null)
            {
                Avatar.texture = avatarTexture;
                break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    IEnumerator FetchNicknameUntilSuccess()
    {
        while (true)
        {
            var nickname = Steam.GetUserNickname(Steam.MySteamID());
            if (nickname != null)
            {
                Nickname.GetComponent<TextMeshProUGUI>().text = nickname;
                break;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    void FetchWinrate()
    {
        var storage = GameObject.FindWithTag("Global").GetComponent<Storage>();
        var userData = storage.GetUserData();
        Winrate.text = CalculatePercentage(userData.wins, userData.gamesPlayed);
    }
    
    string CalculatePercentage(int a, int b)
    {
        if (b == 0)
        {
            return "?";
        }

        double percentage = ((double)a / b) * 100;

        return percentage.ToString("0") + "%";
    }
}
