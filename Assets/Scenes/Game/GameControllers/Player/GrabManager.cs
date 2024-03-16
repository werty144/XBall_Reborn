using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabManager : MonoBehaviour
{
    private bool GrabIntention;

    private MeshRenderer GrabIntentionIndicator;

    private Client Client;
    void Start()
    {
        GrabIntentionIndicator = transform.Find("GrabIntentionIndicator").GetComponent<MeshRenderer>();
        GrabIntentionIndicator.enabled = false;

        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
    }

    public void SetGrabIntention()
    {
        if (ActionRules.IsValidGrab(transform, Client.GetBall().gameObject.transform))
        {
            var action = new GrabAction
            {
                PlayerId = gameObject.GetComponent<PlayerController>().ID
            };
            Client.InputAction(action);
            return;
        }
        
        GrabIntention = true;
        GrabIntentionIndicator.enabled = true;
    }
    
    void Update()
    {
        if (!GrabIntention) return;

        if (!ActionRules.IsValidGrab(transform, Client.GetBall().gameObject.transform)) return;
        
        var action = new GrabAction
        {
            PlayerId = gameObject.GetComponent<PlayerController>().ID
        };
        Client.InputAction(action);
        
        GrabIntention = false;
        GrabIntentionIndicator.enabled = false;
    }
}
