using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanelController : MonoBehaviour
{
    public RawImage AvatarOne;
    public RawImage AvatarTwo;
    public TextMeshProUGUI ScoreOne;
    public TextMeshProUGUI ScoreTwo;
    public TextMeshProUGUI Time;

    private Dictionary<ulong, TextMeshProUGUI> ID2Score = new();
    
    private DateTime startTime;
    private bool isPaused;
    private Stopwatch stopwatch = new Stopwatch();

    private void Start()
    {
        var gameStarter = GameObject.FindWithTag("Global").GetComponent<GameStarter>();
        var userIDOne = gameStarter.Info.MyID;
        var userIDTwo = gameStarter.Info.OpponentID;
        var textureAvatarOne = Steam.GetUserMediumAvatar(userIDOne);
        AvatarOne.texture = textureAvatarOne;
        var textureAvatarTwo = Steam.GetUserMediumAvatar(userIDTwo);
        AvatarTwo.texture = textureAvatarTwo;

        ID2Score[userIDOne.m_SteamID] = ScoreOne;
        ID2Score[userIDTwo.m_SteamID] = ScoreTwo;
    }

    public void OnGoalAttempt(GoalAttempt message)
    {
        foreach (var (id, score) in message.Score)
        {
            ID2Score[id].text = score.ToString();
        }
    }

    void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        var timeElapsed = stopwatch.Elapsed;
        Time.text = $"{(int)timeElapsed.TotalMinutes:0}:{timeElapsed.Seconds % 60:00}";
    }
    
    public void StartTimer()
    {
        stopwatch.Start();
    }

    public void PauseTimer()
    {
        stopwatch.Stop();
    }

    public void ResumeTimer()
    {
        stopwatch.Start();
    }
}
