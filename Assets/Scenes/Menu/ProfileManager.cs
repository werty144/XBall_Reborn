using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public RawImage Avatar;
    public GameObject Nickname;
    public GameObject Rating;
    void Start()
    {
        StartCoroutine(FetchAvatarUntilSuccess());
        StartCoroutine(FetchNicknameUntilSuccess());
        FetchRating();
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
            yield return new WaitForSeconds(0.1f);
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
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FetchRating()
    {
        Rating.GetComponent<TextMeshProUGUI>().text = "228";
    }
}
