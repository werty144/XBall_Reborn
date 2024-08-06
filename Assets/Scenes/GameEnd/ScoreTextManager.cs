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
        var gameStarter = GameObject.FindWithTag("Global").GetComponent<GameStarter>();
        MyScore.text = gameEnder.Score[gameStarter.Info.MyID.m_SteamID].ToString();
        OpponentScore.text = gameEnder.Score[gameStarter.Info.OpponentID.m_SteamID].ToString();
    }
}
