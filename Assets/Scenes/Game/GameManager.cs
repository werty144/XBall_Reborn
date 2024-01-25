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
    
    private Dictionary<uint, PlayerController> Players = new ();
    private CSteamID OpponentID;
    private bool IAmMaster;
    private FieldParams FieldParams;

    // TODO: Initialize on game start
    private uint CurStateNumber;
    private BoundedBuffer<GameState> RecentGameStates;
    private BoundedBuffer<PlayerMovementAction[]> RecentActions;
    private uint BufferedStates = 200;

    private GameState snapshotState_Test;

    public void Setup(SetupInfo setupInfo)
    {
        OpponentID = setupInfo.OpponentID;
        IAmMaster = setupInfo.IAmMaster;
        
        SetFieldParams();
        CreatePlayers(setupInfo.NumberOfPlayers);

        CurStateNumber = 0;
        RecentGameStates = new BoundedBuffer<GameState>(BufferedStates);
        RecentActions = new BoundedBuffer<PlayerMovementAction[]>(BufferedStates);

        if (IAmMaster)
        {
            
        }
    }
    
    // Start is called before the first frame update
    void OnEnable()
    {
        Setup(new SetupInfo{IAmMaster = true, NumberOfPlayers = 3, OpponentID = new CSteamID()});
    }

    // Update is called once per frame
    void Update()
    {
        Assert.IsFalse(RecentGameStates.Has(CurStateNumber), "Cur state is not yet added");
        RecentGameStates.Add(GetGameState());
        Assert.IsTrue(RecentGameStates.Has(CurStateNumber), "Cur state " + CurStateNumber + " is added");
        
        if (!RecentActions.Has(CurStateNumber))
        {
            RecentActions.Add(new PlayerMovementAction[2]);
        }
        CurStateNumber++;
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

    public void InputAction_SetPlayerTarget(GameObject player, Vector2 target)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (!playerController.IsMy) { return; }
        
        playerController.SetTarget(target);
        
        SetPlayerMovementAction(CurStateNumber, playerController.ID, target, IAmMaster ? 0u : 1u);
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
        if (gameStateNumber > CurStateNumber ||
            gameStateNumber != CurStateNumber && !RecentGameStates.Has(gameStateNumber))
        {
            // TODO: Implement buffering actions
            // TODO: Implement catch up mechanism
            Debug.LogWarning("Opponent sent too old or future action");
            return;
        }
        
        if (Players[playerID].IsMy) { return; }
        
        SetPlayerMovementAction(gameStateNumber, playerID, target, !IAmMaster ? 0u : 1u);

        if (gameStateNumber == CurStateNumber)
        {
            Players[playerID].SetTarget(target);
            return;
        }
        
        Merge(gameStateNumber);
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
            UpdatePlayers();
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

    void UpdatePlayers()
    {
        foreach (var player in Players.Values)
        {
            player.Move();
        }
    }

    public void ApplyGameState(GameState state)
    {
        foreach (var playerState in state.PlayerStates)
        {
            Players[playerState.Id].ApplyState(playerState);
        }
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

    public void TestSnapshotState()
    {
        snapshotState_Test = GetGameState();
    }

    public void TestApplyState()
    {
        ApplyGameState(snapshotState_Test);
    }
}
