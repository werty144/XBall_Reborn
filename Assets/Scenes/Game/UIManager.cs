using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameManager GameManager;

    private TextMeshProUGUI PingLabel;
    private TextMeshProUGUI CurrentStateLabel;

    private void OnEnable()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        PingLabel = GameObject.Find("Ping Label").GetComponent<TextMeshProUGUI>();
        CurrentStateLabel = GameObject.Find("Current State Label").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        CurrentStateLabel.text = "State №: " + GameManager.GetCurrentStateNumber();
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
