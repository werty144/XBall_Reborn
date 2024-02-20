using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Steamworks;
using UnityEngine;

public class DebugConsoleCommands : MonoBehaviour
{
    private GameState SnapshotState;
    void Start()
    {
        DebugLogConsole.AddCommand("PeerConnected", "Triggers connection manager's PeerConnected", PeerConnected);
        DebugLogConsole.AddCommand("RemoteProblem", "Triggers connection manager's RemoteProblem", RemoteProblem);
        DebugLogConsole.AddCommand("LocalProblem", "Triggers connection manager's LocalProblem", LocalProblem);
        DebugLogConsole.AddCommand("CloseConnection", "Closes connection", CloseConnection);
        
        DebugLogConsole.AddCommand("EnableDummy", "Enables dummy player", EnableDummy);
        DebugLogConsole.AddCommand("DisableDummy", "Disables dummy player", DisableDummy);
        
        DebugLogConsole.AddCommand("EnableCameraMovement", "EnablesCameraMovement", EnableCameraMovement);
        DebugLogConsole.AddCommand("DisableCameraMovement", "Disables camera movement", DisableCameraMovement);
        
        DebugLogConsole.AddCommand<uint, float>("PlayerSetRotationTargetAngle", "Sets a given rotation angle for a player by ID", SetTargetRotationAngle);
        DebugLogConsole.AddCommand<uint, Vector2>("PlayerSetPosition", "Sets player's position", PlayerSetPosition);
        
        DebugLogConsole.AddCommand("TakeStateSnapshot", "Remembers current game state", TakeStateSnapshot);
        DebugLogConsole.AddCommand("SendSnapshotToClient", "Sends a snapshot state to the client as if it was ReceiveState call", SendSnapshotToClient);
        
        DebugLogConsole.AddCommand<uint>("PlayGrabAnimation", "Plays a grab animation for a specified player", PlayGrabAnimation);
        DebugLogConsole.AddCommand<uint>("PlayThroughAnimation", "Plays a through animation for a specified player", PlayThroughAnimation);
        
        DebugLogConsole.AddCommand<Vector3>("BallPlace", "Places the ball to a given position", BallPlace);
        DebugLogConsole.AddCommand<Vector3>("BallApplyVelocity", "Applies velocity to a ball", BallApplyVelocity);
        DebugLogConsole.AddCommand<uint>("BallSetOwner", "Sets ball's' owner", BallSetOwner);
        DebugLogConsole.AddCommand("BallRemoveOwner", "Removes ball's owner", BallRemoveOwner);
        
        DebugLogConsole.AddCommand("Exit", "Quits the game", Exit);
        
        DebugLogConsole.AddCommand<uint>("TestIsValidGrab", "Tests function ActionRules.IsValidGrab", TestIsValidGrab);
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

    void SetTargetRotationAngle(uint playerID, float angle)
    {
        GameObject.FindWithTag("Server").GetComponent<Server>().GetPlayers()[playerID].SetRotationTargetAngle(angle);
    }

    void TakeStateSnapshot()
    {
        SnapshotState = GameObject.FindWithTag("Client").GetComponent<Client>().GetGameState();
    }

    void SendSnapshotToClient()
    {
        GameObject.FindWithTag("Client").GetComponent<Client>().ReceiveState(SnapshotState);
    }

    void PlayGrabAnimation(uint playerID)
    {
        GameObject.FindWithTag("Server").GetComponent<Server>().GetPlayers()[playerID].PlayGrabAnimation();
    }

    void PlayThroughAnimation(uint playerID)
    {
        GameObject.FindWithTag("Server").GetComponent<Server>().GetPlayers()[playerID].PlayThroughAnimation();
    }

    void BallPlace(Vector3 position)
    {
        var ball = GameObject.FindWithTag("Ball");
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.transform.position = position;
    }

    void BallApplyVelocity(Vector3 velocity)
    {
        var ball = GameObject.FindWithTag("Ball");
        ball.GetComponent<Rigidbody>().velocity = velocity;
    }

    void BallSetOwner(uint playerID)
    {
        var player = GameObject.FindWithTag("Server").GetComponent<Server>().GetPlayers()[playerID];
        var ball = GameObject.FindWithTag("Ball").GetComponent<BallController>();
        ball.SetOwner(player);
    }

    void BallRemoveOwner()
    {
        var ball = GameObject.FindWithTag("Ball").GetComponent<BallController>();
        ball.RemoveOwner();
    }

    void Exit()
    {
        Application.Quit();
    }

    void TestIsValidGrab(uint playerID)
    {
        var ball = GameObject.FindWithTag("Ball").GetComponent<BallController>();
        var player = GameObject.FindWithTag("Server").GetComponent<Server>().GetPlayers()[playerID];
        var result = ActionRules.IsValidGrab(player.transform, ball.transform);
        Debug.Log(result);
    }

    void PlayerSetPosition(uint playerID, Vector2 position)
    {
        var player = GameObject.FindWithTag("Server").GetComponent<Server>().GetPlayers()[playerID];
        player.SetPosition(position);
    }
}
