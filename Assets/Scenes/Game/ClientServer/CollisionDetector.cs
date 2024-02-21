using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private Server Server;

    private void Start()
    {
        Server = GameObject.FindWithTag("Server").GetComponent<Server>();
    }

    private void OnCollisionExit(Collision other)
    {
        Server.CollisionExit();
    }
}
