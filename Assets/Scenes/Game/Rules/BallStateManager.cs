using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BallStateManager
{
    const long GrabImmuneMillis = 1000;
    private Stopwatch GrabImmuneWatch = Stopwatch.StartNew();
    private PlayerController Owner = null;
    
    public bool Grab(PlayerController player)
    {
        if (Owner == null)
        {
            GrabImmuneWatch.Stop();
            GrabImmuneWatch = Stopwatch.StartNew();
            Owner = player;
            return true;
        }

        if (Owner.UserID == player.UserID)
        {
            Owner = player;
            return true;
        }

        if (GrabImmuneWatch.ElapsedMilliseconds >= GrabImmuneMillis)
        {
            GrabImmuneWatch.Stop();
            GrabImmuneWatch = Stopwatch.StartNew();
            Owner = player;
            return true;
        }

        return false;
    }

    public bool Throw(PlayerController player)
    {
        if (Owner == null || Owner.ID != player.ID)
        {
            return false;
        }

        Owner = null;
        return true;
    }
}
