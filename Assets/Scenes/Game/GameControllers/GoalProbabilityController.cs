using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalProbabilityController : MonoBehaviour
{
    public Client Client;
    public Canvas Canvas;
    public Image ProbabilityBar;

    private Camera Camera;
    void Start()
    {
        if (Client == null)
        {
            Canvas.enabled = false;
            enabled = false;
            return;
        }
        Camera = Camera.main;
    }

    private void Update()
    {
        Canvas.transform.LookAt(2 * Canvas.transform.position - Camera.transform.position);
        
        if (!Client.GetBall().Owned)
        {
            Canvas.enabled = false;
            return;
        }

        Canvas.enabled = true;

        var prob = GoalRules.GoalAttemptSuccessProbability(
            Client.GetPlayers(), 
            Client.GetBall().Owner,
            Client.GetBall(), 
            gameObject);
        ProbabilityBar.fillAmount = prob;
    }
}
