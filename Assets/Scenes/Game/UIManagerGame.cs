using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerGame : MonoBehaviour
{
    private GameManager GameManager;

    private TextMeshProUGUI PingLabel;
    private TextMeshProUGUI CurrentStateLabel;

    private DateTime LastFPSUpdate;
    private uint FramesPerLasSecond;

    private void OnEnable()
    {
        LastFPSUpdate = DateTime.Now;
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        PingLabel = GameObject.Find("Ping Label").GetComponent<TextMeshProUGUI>();
        CurrentStateLabel = GameObject.Find("Current State Label").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        FramesPerLasSecond++;
        if (DateTime.Now - LastFPSUpdate >= TimeSpan.FromSeconds(1))
        {
            CurrentStateLabel.text = "FPS: " + FramesPerLasSecond;
            FramesPerLasSecond = 0;
            LastFPSUpdate = DateTime.Now;
        }
    }

    public void OnMenuButton()
    {
        GameManager.GameEnd();
        SceneManager.LoadScene("Menu");
    }

    public void UpdatePing(TimeSpan timeSpan)
    {
        PingLabel.text = "Ping: " + timeSpan.Milliseconds + "ms";
    }
}
