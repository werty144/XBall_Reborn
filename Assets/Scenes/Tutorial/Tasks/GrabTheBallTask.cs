using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GrabTheBallTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public InputManager InputManager;
    public BallController Ball;
    public TextMeshProUGUI mainText;
    public GameObject ballPointer;

    private void OnEnable()
    {
        InputManager.enabled = true;
        ballPointer.SetActive(true);
        mainText.text = "To grab the ball, just come close.\n" + "Grab the ball!";
    }

    private void Update()
    {
        if (Ball.Owned)
        {
            TaskManager.Done();
        }
    }

    private void OnDisable()
    {
        ballPointer.SetActive(false);
    }
}
