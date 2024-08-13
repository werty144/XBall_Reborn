using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManagerGame : MonoBehaviour
{
    public GameObject LoadingScreen;
    public TextMeshProUGUI LoadingScreenText;

    private uint FramesPerLasSecond;

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
        LoadingScreen.SetActive(true);
    }
}
