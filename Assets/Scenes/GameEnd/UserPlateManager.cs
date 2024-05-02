using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserPlateManager : MonoBehaviour
{
    public RawImage MyAvatar;
    public RawImage OpponentAvatar;
    public TextMeshProUGUI MyNickname;
    public TextMeshProUGUI OpponentNickname;

    public Image MyOutline;
    public Image OpponentOutline;

    private GameEnder GameEnder;
    
    void Start()
    {
        GameEnder = GameObject.FindWithTag("Global").GetComponent<GameEnder>();
        
        StartCoroutine(FetchAvatarUntilSuccess());
        StartCoroutine(FetchNicknameUntilSuccess());

        if (GameEnder.Winner == GameEnder.MyId)
        {
            OpponentOutline.color = Color.gray;
        }
        else
        {
            MyOutline.color = Color.gray;
        }
    }

    IEnumerator FetchAvatarUntilSuccess()
    {
        while (true)
        {
            var myAvatarTexture = Steam.GetUserLargeAvatar(GameEnder.MyId);
            var opponentAvatarTexture = Steam.GetUserLargeAvatar(GameEnder.OpponentID);
            if (myAvatarTexture != null && opponentAvatarTexture != null)
            {
                MyAvatar.texture = myAvatarTexture;
                OpponentAvatar.texture = opponentAvatarTexture;
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator FetchNicknameUntilSuccess()
    {
        while (true)
        {
            var myNickname = Steam.GetUserNickname(GameEnder.MyId);
            var opponentNickname = Steam.GetUserNickname(GameEnder.OpponentID);
            if (myNickname != null && opponentNickname != null)
            {
                MyNickname.text = myNickname;
                OpponentNickname.text = opponentNickname;
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
