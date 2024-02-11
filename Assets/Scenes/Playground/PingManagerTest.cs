using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class PingManagerTest : PingManager
{
    public override TimeSpan GetPingToUser(CSteamID userID)
    {
        return TimeSpan.Zero;
    }
}
