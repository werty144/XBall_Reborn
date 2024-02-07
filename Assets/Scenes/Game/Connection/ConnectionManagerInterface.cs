using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public interface ConnectionManager
{
    public void OnConnected(HSteamNetConnection connection);
    public void OnRemoteProblem();
    public void OnLocalProblem();

    public void OnUnknownProblem();
    public void OnClosedByPeer();
    public void SendMessage(byte[] message);

    public CSteamID GetPeerID();
}
