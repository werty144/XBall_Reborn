using System;
using System.Transactions;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    public ulong UserID;
    private Server Server;

    private void Start()
    {
        Server = GameObject.FindWithTag("Server").GetComponent<Server>();
    }

    private void OnCollisionEnter(Collision other)
    {
        var serverLayer = LayerMask.NameToLayer("Server");
        if (other.gameObject.layer != serverLayer) { return; }
        
        Server.OnGoalAttempt(UserID);
    }
}
