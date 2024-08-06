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
    private GameStarter GameStarter;
    
    void Start()
    {
        GameEnder = GameObject.FindWithTag("Global").GetComponent<GameEnder>();
        GameStarter = GameObject.FindWithTag("Global").GetComponent<GameStarter>();
        
        StartCoroutine(FetchAvatarUntilSuccess());
        StartCoroutine(FetchNicknameUntilSuccess());

        if (GameEnder.Winner == GameStarter.Info.MyID)
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
            var myAvatarTexture = Steam.GetUserLargeAvatar(GameStarter.Info.MyID);
            var opponentAvatarTexture = Steam.GetUserLargeAvatar(GameStarter.Info.OpponentID);
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
            var myNickname = Steam.GetUserNickname(GameStarter.Info.MyID);
            var opponentNickname = Steam.GetUserNickname(GameStarter.Info.OpponentID);
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
