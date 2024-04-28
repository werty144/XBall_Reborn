using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GrabManager : MonoBehaviour
{
    private Stopwatch Cooldown;

    private Client Client;
    public PlayerController ThisPlayer;
    void Start()
    {
        if (!ThisPlayer.IsMy)
        {
            enabled = false;
            return;
        }
        Cooldown = Stopwatch.StartNew();

        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
    }
    
    void Update()
    {
        if (Cooldown.ElapsedMilliseconds < ActionRulesConfig.GrabCooldown) return;

        if (!ActionRules.IsValidGrab(transform, Client.GetBall().gameObject.transform)) return;

        if (Client.GetBall().Owned && Client.GetBall().Owner.UserID == ThisPlayer.UserID) return;
        
        var action = new GrabAction
        {
            PlayerId = ThisPlayer.ID
        };
        
        Client.InputAction(action);
        
        Cooldown.Restart();
    }

    public void RestartCooldown()
    {
        Cooldown.Restart();
    }
}
