using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;

class Cooldown
{
    private Stopwatch watch = Stopwatch.StartNew();
    private float cooldownMilllis = 0;

    public void SetCooldown(float duration)
    {
        watch.Restart();
        cooldownMilllis = duration;
    }

    public bool CooldwonElapsed()
    {
        return watch.ElapsedMilliseconds > cooldownMilllis;
    }
}

public class GrabManager : MonoBehaviour
{
    private Cooldown Cooldown;

    private Client Client;
    public PlayerController ThisPlayer;
    void Start()
    {
        if (!ThisPlayer.IsMy)
        {
            enabled = false;
            return;
        }

        Cooldown = new Cooldown();

        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
    }
    
    void Update()
    {
        if (!Cooldown.CooldwonElapsed()) return;

        if (!ActionRules.IsValidGrab(transform, Client.GetBall().gameObject.transform)) return;

        if (Client.GetBall().Owned && Client.GetBall().Owner.UserID == ThisPlayer.UserID) return;
        
        var action = new GrabAction
        {
            PlayerId = ThisPlayer.ID
        };
        
        Client.InputAction(action);
        
        Cooldown.SetCooldown(ActionRulesConfig.GrabCooldown);
    }

    public void SetCooldownMillis(float cooldownMillis)
    {
        if (!enabled) return;
        Cooldown.SetCooldown(cooldownMillis);
    }
}
