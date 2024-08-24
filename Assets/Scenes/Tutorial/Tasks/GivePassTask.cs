using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GivePassTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public TextMeshProUGUI mainText;
    public GameObject MyPlayer2;
    public InputManager InputManager;
    public GameObject anyKey;
    public BallController Ball;

    private int step = 1;

    private void OnEnable()
    {
        InputManager.enabled = false;
        MyPlayer2.SetActive(true);
        mainText.text = "Look, there is another player!";
        anyKey.SetActive(true);
    }

    private void Update()
    {
        switch (step)
        {
            case 1:
                if (Input.anyKeyDown)
                {
                    mainText.text = "To give a pass to a player, you need to throw a ball to him.";
                    step++;
                }
                break;
            case 2:
                if (Input.anyKeyDown)
                {
                    mainText.text = "To throw a ball, press Q and left click where you want to throw it.";
                    step++;
                }
                break;
            case 3:
                if (Input.anyKeyDown)
                {
                    mainText.text = "Your throw is less precise if it's long or if you throw backward.";
                    step++;
                }
                break;
            case 4:
                if (Input.anyKeyDown)
                {
                    mainText.text = "Give a pass!";
                    InputManager.enabled = true;
                    anyKey.SetActive(false);
                    step++;
                }
                break;
            case 5:
                if (Ball.Owned && Ball.Owner.ID == 2)
                {
                    TaskManager.Done();
                }
                break;
        }
    }
}
