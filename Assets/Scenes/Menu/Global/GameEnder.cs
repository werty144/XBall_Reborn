using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

public class GameEnder : MonoBehaviour
{
    public Dictionary<ulong, int> Score;
    public CSteamID Winner;
}
