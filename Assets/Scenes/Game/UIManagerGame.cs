using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerGame : MonoBehaviour
{
    private P2PBase P2PManager;
    private TextMeshProUGUI PingLabel;
    private TextMeshProUGUI CurrentStateLabel;

    private DateTime LastFPSUpdate;
    private uint FramesPerLasSecond;

    private void Awake()
    {
        LastFPSUpdate = DateTime.Now;
        PingLabel = GameObject.Find("Ping Label").GetComponent<TextMeshProUGUI>();
        CurrentStateLabel = GameObject.Find("Current State Label").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        P2PManager = GameObject.FindWithTag("P2P").GetComponent<P2PBase>();
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
        PingLabel.text = "Ping: " + P2PManager.GetPing().Milliseconds + "ms";
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
