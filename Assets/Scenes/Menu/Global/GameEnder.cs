using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

public class GameEnder : MonoBehaviour
{
    public CSteamID MyId;
    public CSteamID OpponentID;
    public Dictionary<ulong, int> Score;
    public CSteamID Winner;
}
