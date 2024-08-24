using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StealTheBallTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public GameObject OpponentPlayerObject;
    public InputManager InputManager;
    public TextMeshProUGUI mainText;
    public GameObject anyKey;
    public BallController Ball;
    public ClientTutorial Client;
    public GrabManager MyPlayer2GrabManager;
    
    private int step = 1;
    private PlayerController OpponentPlayer;

    private void OnEnable()
    {
        InputManager.enabled = false;
        OpponentPlayerObject.SetActive(true);
        OpponentPlayer = OpponentPlayerObject.GetComponent<PlayerController>();
        mainText.text = "Oops, there is an opponent!";

        var ballPosition = Ball.gameObject.transform.position;
        var target = new Vector2(ballPosition.x, ballPosition.z);
        OpponentPlayer.SetMovementTarget(target);
    }

    private void Update()
    {
        switch (step)
        {
            case 1:
                if (ActionRules.IsValidGrab(OpponentPlayer.transform, Ball.transform))
                {
                    OpponentPlayer.Stop();
                    var grabAction = new GrabAction
                    {
                        PlayerId = OpponentPlayer.ID
                    };
                    Client.InputAction(grabAction);
                    MyPlayer2GrabManager.SetCooldownMillis(5000f);
                    step++;
                }
                break;
            case 2:
                if (Ball.Owned && Ball.Owner.ID == OpponentPlayer.ID)
                {
                    OpponentPlayer.SetMovementTarget(new Vector2(-5, 3));
                    step++;
                }
                break;
            case 3:
                if (!OpponentPlayer.isMoving)
                {
                    mainText.text = "We need to steal the ball back!";
                    anyKey.SetActive(true);
                    step++;
                }
                break;
            case 4:
                if (Input.anyKeyDown)
                {
                    mainText.text = "To steal the ball, get close to the opponent.";
                    InputManager.enabled = true;
                    step++;
                }
                break;
            case 5:
                if (Input.anyKeyDown)
                {
                    anyKey.SetActive(false);
                    mainText.text = "It may not work the first time. Go try.";
                    step++;
                }
                break;
            case 6:
                if (Ball.Owned && Ball.Owner.IsMy)
                {
                    OpponentPlayerObject.GetComponent<GrabManager>().enabled = false;
                    TaskManager.Done();
                }
                break;
        }
    }
}
