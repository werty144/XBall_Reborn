using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Steamworks;
using UnityEngine;

public class DebugConsoleCommands : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DebugLogConsole.AddCommand("PeerConnected", "Triggers connection manager's PeerConnected", PeerConnected);
        DebugLogConsole.AddCommand("RemoteProblem", "Triggers connection manager's RemoteProblem", RemoteProblem);
        DebugLogConsole.AddCommand("LocalProblem", "Triggers connection manager's LocalProblem", LocalProblem);
        DebugLogConsole.AddCommand("CloseConnection", "Closes connection", CloseConnection);
    }

    void PeerConnected()
    {
        GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>().OnConnected(new HSteamNetConnection());
    }

    void RemoteProblem()
    {
        GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>().OnRemoteProblem();
    }
    
    void LocalProblem()
    {
        GameObject.FindWithTag("P2P").GetComponent<ConnectionManager>().OnLocalProblem();
    }

    void CloseConnection()
    {
        GameObject.FindWithTag("P2P").GetComponent<ConnectionManagerBase>().CloseConnection();
    }
}
