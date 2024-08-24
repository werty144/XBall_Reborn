using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public CameraController CameraController;
    public InputManager InputManager;
    public GameObject Ball;
    public TextMeshProUGUI mainText;
    public GameObject anyKeyText;

    private bool anyKeyPressed = false;
    private bool done = false;

    private void OnEnable()
    {
        InputManager.enabled = false;
        Ball.SetActive(true);
        mainText.text = "Hey look, there is a ball!";
        anyKeyText.SetActive(true);
    }

    private void Update()
    {
        if (done) return;
        if (!anyKeyPressed)
        {
            if (Input.anyKeyDown)
            {
                anyKeyPressed = true;
                anyKeyText.SetActive(false);
                CameraController.enabled = true;
                mainText.text = "To move camera forward, move your pointer to the top of the screen.\n" +
                                "Move camera forward.";
            }
        }
        else
        {
            if (Input.mousePosition.y > Screen.height * 0.9)
            {
                done = true;
                Invoke(nameof(Done), 1f);
            }
        }
    }

    void Done()
    {
        TaskManager.Done();
    }
}
