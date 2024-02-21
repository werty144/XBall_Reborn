using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class DummyPlayer : MonoBehaviour
{
    public GameObject slider;

    private GameStarter gameStarter;
    private MessageManagerTest messageManager;
    
    private Dictionary<uint, PlayerController> AllPlayers = new ();
    private List<PlayerController> MyPlayers = new();
    private BallController Ball;
    private TimeSpan ActionFrequency = TimeSpan.FromSeconds(1);
    private DateTime LastAction;

    private TimeSpan Ping = TimeSpan.Zero;

    private void Awake()
    {
        enabled = false;
        messageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManagerTest>();
    }

    void OnEnable()
    {
        CreatePlayers();
        CreateBall();
        messageManager.DummyReady();
    }

    public void Disable()
    {
        enabled = false;
    }

    public void Enable()
    {
        enabled = true;
    }

    void CreatePlayers()
    {
        foreach (var player in AllPlayers.Values)
        {
            Destroy(player.gameObject);
        }

        var playerPrefab = Resources.Load<GameObject>("Player Variant");
        var layerMask = LayerMask.NameToLayer("Dummy");
        var server = GameObject.FindWithTag("Server").GetComponent<Server>();
        foreach (var serverPlayer in server.GetPlayers().Values)
        {
            var localPlayer = Instantiate(playerPrefab, serverPlayer.transform.position, serverPlayer.transform.rotation);
            localPlayer.layer = layerMask;
            var playerController = localPlayer.GetComponent<PlayerController>();
            playerController.ID = serverPlayer.ID;
            AllPlayers[playerController.ID] = playerController;
            if (serverPlayer.ID % 2 == 1)
            {
                playerController.IsMy = true;
                MyPlayers.Add(playerController);
            }
        }
        
        foreach (var playerController in AllPlayers.Values)
        {
            foreach (Renderer renderer in playerController.GetComponentsInChildren<Renderer>())
            {
                // renderer.enabled = true;
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color;
                        if (playerController.ID % 2 == 0)
                        {
                            color = PlayerConfig.MyColor;
                        }
                        else
                        {
                            color = PlayerConfig.OpponentColor;
                        }

                        color.a = 0.5f;
                        material.color = color;
                    }
                }
            }
        }
    }

    Dictionary<uint, PlayerController> GetPlayers()
    {
        return AllPlayers;
    }

    void CreateBall()
    {
        Destroy(Ball);
        var server = GameObject.FindWithTag("Server").GetComponent<Server>();
        var serverBall = server.GetBall();
        var ballPrefab = Resources.Load<GameObject>("Ball Variant");
        var ballObject = Instantiate(ballPrefab,serverBall.transform.position, server.transform.rotation);
        
        ballObject.GetComponentInChildren<Renderer>().enabled = true;
        
        int collisionLayer = LayerMask.NameToLayer("Dummy");
        ballObject.layer = collisionLayer;
        
        Ball = ballObject.GetComponent<BallController>();
    }
    
    public void ReceiveState(GameState gameState)
    {
        CorrectOpponentPlayers(gameState);
    }

    private void CorrectOpponentPlayers(GameState referenceState)
    {
        foreach (var playerState in referenceState.PlayerStates)
        {
            var player = GetPlayers()[playerState.Id];
            if (player.IsMy) {continue;}

            if (playerState.IsMoving)
            {
                player.SetMovementTarget(new Vector2(playerState.TargetX, playerState.TargetY));
            }
            else
            {
                player.Stop();
                var currentPosition = player.GetPosition();
                var referencePosition = new Vector2(playerState.X, playerState.Y);
                if (Vector2.Distance(currentPosition, referencePosition) >= PlayerConfig.Radius)
                {
                    player.SetMovementTarget(referencePosition);
                }

                if (Mathf.Abs(player.GetAngle() - playerState.RotationAngle) >= 0.1)
                {
                    player.SetRotationTargetAngle(playerState.RotationAngle);
                }
            }
        }
    }

    void Update()
    {
        if (DateTime.Now - LastAction >= ActionFrequency)
        {
            LastAction = DateTime.Now;
            RandomMove();
        }
    }

    void RandomMove()
    {
        var random = new Random();
        var playerIndex = random.Next(0, MyPlayers.Count);
        var player = MyPlayers[playerIndex];
        var playerID = player.ID;  
        var randX = random.Next(0, 20) - 10;
        var randY = random.Next(0, 30) - 15;
        var action = new PlayerMovementAction
        {
            PlayerId = playerID,
            X = randX,
            Y = randY
        };
        player.SetMovementTarget(new Vector2(randX, randY));
        messageManager.DummySendAction(action);
    }
}
