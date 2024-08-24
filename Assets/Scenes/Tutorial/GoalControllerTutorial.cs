using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalControllerTutorial : MonoBehaviour
{
    public ClientTutorial Client;
    
    private void OnCollisionEnter(Collision other)
    {
        Client.ReceiveGoalAttempt(new GoalAttempt());
    }
}
