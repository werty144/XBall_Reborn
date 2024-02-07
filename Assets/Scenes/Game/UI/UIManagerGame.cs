using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerGame : MonoBehaviour
{
    private PingManager PingManager;
    private TextMeshProUGUI PingLabel;
    private TextMeshProUGUI CurrentStateLabel;
    private GameObject LoadingScreen;
    private TextMeshProUGUI LoadingScreenText;

    private DateTime LastFPSUpdate;
    private uint FramesPerLasSecond;

    private void Awake()
    {
        LastFPSUpdate = DateTime.Now;
        PingLabel = GameObject.Find("Ping Label").GetComponent<TextMeshProUGUI>();
        CurrentStateLabel = GameObject.Find("Current State Label").GetComponent<TextMeshProUGUI>();
        LoadingScreen = GameObject.Find("Loading Screen");
        LoadingScreenText = LoadingScreen.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void DisplayLoading()
    {
        LoadingScreen.SetActive(true);
        LoadingScreenText.text = "Loading...";
    }

    public void DisplayLocalProblem()
    {
        LoadingScreen.SetActive(true);
        LoadingScreenText.text = "Internet issue";
    }

    public void DisplayPeerDropped()
    {
        LoadingScreen.SetActive(true);
        LoadingScreenText.text = "Opponent disconnected";
    }

    public void DisplayUnknownProblem()
    {
        LoadingScreen.SetActive(true);
        LoadingScreenText.text = "Unknown problem";
    }

    public void RemoveScreen()
    {
        LoadingScreen.SetActive(false);
    }

    private void Start()
    {
        PingManager = GameObject.FindWithTag("P2P").GetComponent<PingManager>();
    }

    private void Update()
    {
        UpdateFPS();
        UpdatePing();
    }

    public void OnMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }

    public void UpdatePing()
    {
        PingLabel.text = "Ping: " + PingManager.GetPing().Milliseconds + "ms";
    }

    private void UpdateFPS()
    {
        FramesPerLasSecond++;
        if (DateTime.Now - LastFPSUpdate >= TimeSpan.FromSeconds(1))
        {
            CurrentStateLabel.text = "FPS: " + FramesPerLasSecond;
            FramesPerLasSecond = 0;
            LastFPSUpdate = DateTime.Now;
        }
    }
}
