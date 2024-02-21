using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class PingManagerTest : PingManager
{
    private TimeSpan DummyPing = TimeSpan.Zero;
    private TimeSpan MyPing = TimeSpan.Zero;
    public override TimeSpan GetPingToUser(CSteamID userID)
    {
        if (userID == new CSteamID(0))
        {
            return MyPing;
        }
        else
        {
            return DummyPing;
        }
    }

    public void SetDummyPing(int millis)
    {
        DummyPing = TimeSpan.FromMilliseconds(millis);
    }

    public void SetMyPing(int millis)
    {
        MyPing = TimeSpan.FromMilliseconds(millis);
    }
}
