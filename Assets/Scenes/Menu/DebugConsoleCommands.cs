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
        
        DebugLogConsole.AddCommand("RemovePlayers", "Removes players", RemovePlayers);
        DebugLogConsole.AddCommand<int>("CreatePlayers", "Creates players", CreatePlayers);
        
        DebugLogConsole.AddCommand("EnableDummy", "Enables dummy player", EnableDummy);
        DebugLogConsole.AddCommand("DisableDummy", "Disables dummy player", DisableDummy);
        
        DebugLogConsole.AddCommand("EnableCameraMovement", "EnablesCameraMovement", EnableCameraMovement);
        DebugLogConsole.AddCommand("DisableCameraMovement", "Disables camera movement", DisableCameraMovement);
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

    void RemovePlayers()
    {
        GameObject.FindWithTag("Server").GetComponent<ServerTest>().RemovePlayers();
        GameObject.FindWithTag("Dummy").GetComponent<DummyPlayer>().Disable();
    }

    void CreatePlayers(int n)
    {
        GameObject.FindWithTag("Client").GetComponent<ClientTest>().CreatePlayers(n);
        GameObject.FindWithTag("Server").GetComponent<ServerTest>().GrabPlayers();
    }
    
    void DisableDummy()
    {
        GameObject.FindWithTag("Dummy").GetComponent<DummyPlayer>().Disable();
    }

    void EnableDummy()
    {
        GameObject.FindWithTag("Dummy").GetComponent<DummyPlayer>().Enable();
    }

    void DisableCameraMovement()
    {
        GameObject.FindWithTag("MainCamera").GetComponent<CameraController>().enabled = false;
    }
    
    void EnableCameraMovement()
    {
        GameObject.FindWithTag("MainCamera").GetComponent<CameraController>().enabled = true;
    }
}
