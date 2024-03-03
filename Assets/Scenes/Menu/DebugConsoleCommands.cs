using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Steamworks;
using Unity.VisualScripting;
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
        
        DebugLogConsole.AddCommand("DummyShow", "Shows dummy's players", DummyShow);
        DebugLogConsole.AddCommand("DummyHide", "Hides dummy's players", DummyHide);
        DebugLogConsole.AddCommand<uint>("DummyGrab", "Sets a grab intention for a given dummy player", DummyGrab);
        DebugLogConsole.AddCommand("StartGameDummy", "Ment to be called from Menu. Switches to the Game Scene with dummy", StartGameDummy);
        
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
        DebugLogConsole.AddCommand<Vector3>("BallThrowTo", "Throws a ball to a given destination", BallThrowTo);
        DebugLogConsole.AddCommand<uint>("BallSetOwner", "Sets ball's' owner", BallSetOwner);
        DebugLogConsole.AddCommand("BallRemoveOwner", "Removes ball's owner", BallRemoveOwner);
        
        DebugLogConsole.AddCommand<int>("PingSetMy", "Sets my ping", PingSetMy);
        DebugLogConsole.AddCommand<int>("PingSetDummy", "Sets dummy's ping", PingSetDummy);
        
        DebugLogConsole.AddCommand("HideServerView", "Hides server's players and ball", HideServerView);
        DebugLogConsole.AddCommand("ShowServerView", "Shows server's players and ball", ShowServerView);
        DebugLogConsole.AddCommand("ShowClientServerView", "Shows a local server simulation", ShowClientServerView);
        DebugLogConsole.AddCommand("HideClientServerView", "Hides a local server simulation", HideClientServerView);
        
        DebugLogConsole.AddCommand("Exit", "Quits the game", Exit);
        
        DebugLogConsole.AddCommand<uint>("TestIsValidGrab", "Tests function ActionRules.IsValidGrab", TestIsValidGrab);
        DebugLogConsole.AddCommand("PlaySuccessAnimation", "", TestPlayGoalSuccessAnimation);
    }

    void TestPlayGoalSuccessAnimation()
    {
        foreach (var goal in GameObject.FindGameObjectsWithTag("Goal"))
        {
            goal.GetComponent<GoalController>().PlaySuccessAnimation();
        }
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
    
    void DummyHide()
    {
        GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>().Hide();
    }

    void DummyShow()
    {
        GameObject.FindWithTag("Dummy").GetComponent<ClientDummy>().Show();
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
        var ball = GameObject.FindWithTag("Server").GetComponent<Server>().GetBall();
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.transform.position = position;
    }

    void BallApplyVelocity(Vector3 velocity)
    {
        var ball = GameObject.FindWithTag("Server").GetComponent<Server>().GetBall();
        ball.GetComponent<Rigidbody>().velocity = velocity;
    }

    void BallThrowTo(Vector3 destination)
    {
        Instantiate(Resources.Load<GameObject>("TargetMarker/TargetMarker"), destination, Quaternion.identity);
        
        var ball = GameObject.FindWithTag("Server").GetComponent<Server>().GetBall();
        ball.ThrowTo(destination);
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

    void PingSetMy(int millis)
    {
        GameObject.FindWithTag("P2P").GetComponent<MessageManagerTest>().SetMyPing(millis);
        GameObject.FindWithTag("P2P").GetComponent<PingManagerTest>().SetMyPing(millis);
    }
    
    void PingSetDummy(int millis)
    {
        GameObject.FindWithTag("P2P").GetComponent<MessageManagerTest>().SetDummyPing(millis);
        GameObject.FindWithTag("P2P").GetComponent<PingManagerTest>().SetDummyPing(millis);
    }

    void HideServerView()
    {
        var server = GameObject.FindWithTag("Server").GetComponent<Server>();
        foreach (var player in server.GetPlayers().Values)
        {
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        server.GetBall().GetComponentInChildren<Renderer>().enabled = false;
    }

    void ShowServerView()
    {
        var server = GameObject.FindWithTag("Server").GetComponent<Server>();
        foreach (var player in server.GetPlayers().Values)
        {
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }

        server.GetBall().GetComponentInChildren<Renderer>().enabled = true;
    }

    void ShowClientServerView()
    {
        var client = GameObject.FindWithTag("Client").GetComponent<Client>();
        foreach (var player in client.ServerPlayers.Values)
        {
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }
        client.ServerBall.GetComponentInChildren<Renderer>().enabled = true;
    }

    void HideClientServerView()
    {
        var client = GameObject.FindWithTag("Client").GetComponent<Client>();
        foreach (var player in client.ServerPlayers.Values)
        {
            foreach (Renderer renderer in player.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
        client.ServerBall.GetComponentInChildren<Renderer>().enabled = false;
    }

    void StartGameDummy()
    {
        var gameStarter = GameObject.FindWithTag("Global").GetComponent<GameStarter>();
        gameStarter.IsTest = true;
        var setUpInfo = new SetupInfo
        {
            IAmMaster = true,
            MyID = new CSteamID(0),
            NumberOfPlayers = 3,
            OpponentID = new CSteamID(1)
        };
        gameStarter.Initiate(setUpInfo);
    }

    void DummyGrab(uint playerID)
    {
        GameObject.FindWithTag("Dummy").GetComponent<DummyUser>().SetPlayerGrabIntention(playerID);
    }
}
