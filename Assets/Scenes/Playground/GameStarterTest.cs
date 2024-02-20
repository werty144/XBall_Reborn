using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class GameStarterTest : GameStarter
{
    private void Awake()
    {
        IsTest = true;

        Info = new SetupInfo
        {
            IAmMaster = true,
            NumberOfPlayers = 3,
            OpponentID = new CSteamID(1),
            MyID = new CSteamID(0)
        };
    }
}
