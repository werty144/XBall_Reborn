using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WelcomeTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public TextMeshProUGUI mainText;
    public GameObject anyKeyText;

    private void OnEnable()
    {
        mainText.text = "Welcome to XBall tutorial!";
        anyKeyText.SetActive(true);
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            TaskManager.Done();
        }
    }

    private void OnDisable()
    {
        anyKeyText.SetActive(false);
    }
}
