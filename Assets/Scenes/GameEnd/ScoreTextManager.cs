using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTextManager : MonoBehaviour
{
    public TextMeshProUGUI MyScore;
    public TextMeshProUGUI OpponentScore;
    
    void Start()
    {
        var gameEnder = GameObject.FindWithTag("Global").GetComponent<GameEnder>();
        MyScore.text = gameEnder.Score[gameEnder.MyId.m_SteamID].ToString();
        OpponentScore.text = gameEnder.Score[gameEnder.OpponentID.m_SteamID].ToString();
    }
}
