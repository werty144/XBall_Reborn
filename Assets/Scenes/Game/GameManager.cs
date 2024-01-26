using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;

public struct SetupInfo
{
    public int NumberOfPlayers;
    public CSteamID OpponentID;
    public bool IAmMaster;
}

public struct FieldParams
{
    public float Width;
    public float Length;
}

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    
    private P2P P2PManager;
    private GameStarter GameStarter;
    private UIManagerGame _uiManagerGame;
    
    private Dictionary<uint, PlayerController> Players = new ();
    private CSteamID OpponentID;
    private bool IAmMaster;
    private FieldParams FieldParams;
    private uint CurStateNumber;
    private BoundedBuffer<GameState> RecentGameStates;
    private BoundedBuffer<PlayerMovementAction[]> RecentActions;
    private uint BufferedStates = 200;
    private bool GameStarted;

    // ----------------------------- SETUP --------------------------------
    public void Setup(SetupInfo setupInfo)
    {
        Debug.Log("Setting up game manager");
        OpponentID = setupInfo.OpponentID;
        IAmMaster = setupInfo.IAmMaster;
        
        SetFieldParams();
        CreatePlayers(setupInfo.NumberOfPlayers);

        CurStateNumber = 0;
        RecentGameStates = new BoundedBuffer<GameState>(BufferedStates);
        RecentActions = new BoundedBuffer<PlayerMovementAction[]>(BufferedStates);
        
        GameStarter.GameManagerReady(this);
    }
    
    void OnEnable()
    {
        var global = GameObject.FindWithTag("Global");
        P2PManager = global.GetComponent<P2P>();
        GameStarter = global.GetComponent<GameStarter>();
        _uiManagerGame = GameObject.FindWithTag("UIManager").GetComponent<UIManagerGame>();
        
        var setupInfo = global.GetComponent<GameStarter>().GetSetupInfo();
        Setup(setupInfo);
    }
    
    void CreatePlayers(int n)
    {
        byte spareID = 0;
        float masterZ = -FieldParams.Length / 4;
        float followerZ = FieldParams.Length / 4;
        for (int i = 0; i < n; i++)
        {
            var x = FieldParams.Width * (i + 1) / (n + 1) - FieldParams.Width / 2;
            
            var masterPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, masterZ), Quaternion.identity);
            var controller = masterPlayer.GetComponent<PlayerController>();
            controller.Initialize(IAmMaster, spareID);
            Players[spareID] = controller;
            spareID++;
            
            var followerPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.identity);
            var followerContorller = followerPlayer.GetComponent<PlayerController>();
            followerContorller.Initialize(!IAmMaster, spareID);
            Players[spareID] = followerContorller;
            spareID++;
        }
    }

    void SetFieldParams()
    {
        var defaultPlaneWidth = 10;
        var defaultPlaneLength = 10;
        
        var floor = GameObject.FindWithTag("Floor");
        var scale = floor.GetComponent<Transform>().localScale;
        FieldParams.Width = scale.x * defaultPlaneWidth;
        FieldParams.Length = scale.z * defaultPlaneLength;
    }

    public void StartGame()
    {
        GameStarted = true;
    }
    
    // -------------------------------------- SERVER-CLIENT ------------------------------
    void Update()
    {
        if (!GameStarted) {return;}
        
        Assert.IsFalse(RecentGameStates.Has(CurStateNumber), "Cur state is not yet added");
        RecentGameStates.Add(GetGameState());
        Assert.IsTrue(RecentGameStates.Has(CurStateNumber), "Cur state " + CurStateNumber + " is added");
        
        if (!RecentActions.Has(CurStateNumber))
        {
            RecentActions.Add(new PlayerMovementAction[2]);
        }
        CurStateNumber++;
    }

    public void InputAction_SetPlayerTarget(GameObject player, Vector2 target)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (!playerController.IsMy) { return; }

        // Optimistic execution in case we are a follower
        playerController.SetTarget(target);
        
        if (IAmMaster)
        {
            var curState = GetGameState();
            P2PManager.SendGameStateMessage(OpponentID, curState);
        }
        else
        {
            P2PManager.SendPlayerMovementAction(OpponentID, 
                new PlayerMovementAction
                {
                    GameStateNumber = CurStateNumber,
                    Id = playerController.ID,
                    X = target.x,
                    Y = target.y
                });
        }
        
        // SetPlayerMovementAction(CurStateNumber, playerController.ID, target, IAmMaster ? 0u : 1u);
    }

    public void SetPlayerMovementAction(uint gameStateNumber, uint playerID, Vector2 target, uint actionPriority)
    {
        Assert.IsTrue(gameStateNumber == CurStateNumber || RecentActions.Has(gameStateNumber));
        
        PlayerMovementAction[] curActions;
        if (RecentActions.Has(gameStateNumber))
        {
            curActions = RecentActions.Get(gameStateNumber);
        }
        else
        {
            curActions = new PlayerMovementAction[2];
            RecentActions.Add(curActions);
        }

        var myAction = new PlayerMovementAction();
        myAction.Id = playerID;
        myAction.X = target.x;
        myAction.Y = target.y;
        
        
        curActions[actionPriority] = myAction;
    }

    public void OpponentAction_SetPlayerTarget(uint gameStateNumber, uint playerID, Vector2 target)
    {
        Assert.IsTrue(IAmMaster, "Only process opponent's actions as a server");
        
        if (Players[playerID].IsMy) { return; }
        
        // SetPlayerMovementAction(gameStateNumber, playerID, target, !IAmMaster ? 0u : 1u);
        
        Players[playerID].SetTarget(target);
        var curState = GetGameState();
        P2PManager.SendGameStateMessage(OpponentID, curState);
        
        // Merge(gameStateNumber);
    }

    public void Merge(uint gameStateNumber)
    {
        Assert.IsTrue(gameStateNumber < CurStateNumber);
        // Do a roll back
        var headState = RecentGameStates.Get(gameStateNumber);
        ApplyGameState(headState);
        
        
        //Sequentially apply all actions
        var mergingState = gameStateNumber;
        Physics.autoSimulation = false;
        while (mergingState < CurStateNumber)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            ApplyActions(mergingState);
            foreach (var player in Players.Values)
            {
                player.Move();
            }
            mergingState++;
        }
        Physics.autoSimulation = true;
    }
    
    void ApplyActions(uint gameStateNumber)
    {
        var actions = RecentActions.Get(gameStateNumber);
        for (int i = 0; i < 2; i++)
        {
            if (actions[i] != null)
            {
                var target = new Vector2(actions[i].X, actions[i].Y);
                Players[actions[i].Id].SetTarget(target);
            }
        }
    }

    public void ApplyGameState(GameState state)
    {
        Assert.IsFalse(IAmMaster, "Only apply game states as a follower");
        foreach (var playerState in state.PlayerStates)
        {
            Players[playerState.Id].ApplyState(playerState);
        }
    }

    public void GameEnd()
    {
        P2PManager.CloseConnection();
    }

    public void LastPingTook(TimeSpan timeSpan)
    {
        _uiManagerGame.UpdatePing(timeSpan);
    }
    
    // ------------------------ GETTERS ------------------------- //
    public uint GetCurrentStateNumber()
    {
        return CurStateNumber;
    }
    
    public GameState GetGameState()
    {
        GameState gameState = new GameState();
        foreach (var player in Players.Values)
        {
            gameState.PlayerStates.Add(player.GetState());
        }

        return gameState;
    }
    
    public bool IsMaster()
    {
        return IAmMaster;
    }
}
