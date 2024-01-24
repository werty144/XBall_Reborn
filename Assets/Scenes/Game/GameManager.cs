using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

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
    
    private Dictionary<uint, GameObject> Players = new ();
    private CSteamID OpponentID;
    private bool IAmMaster;
    private FieldParams FieldParams;

    private GameState snapshotState_Test;

    public void Setup(SetupInfo setupInfo)
    {
        OpponentID = setupInfo.OpponentID;
        IAmMaster = setupInfo.IAmMaster;
        
        SetFieldParams();
        CreatePlayers(setupInfo.NumberOfPlayers);

        if (IAmMaster)
        {
            
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Setup(new SetupInfo{IAmMaster = true, NumberOfPlayers = 3, OpponentID = new CSteamID()});
        }
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
            masterPlayer.GetComponent<PlayerController>().Initialize(IAmMaster, spareID);
            Players[spareID] = masterPlayer;
            spareID++;
            
            var followerPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.identity);
            followerPlayer.GetComponent<PlayerController>().Initialize(!IAmMaster, spareID);
            Players[spareID] = followerPlayer;
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
        if (!player.GetComponent<PlayerController>().IsMy) { return; }
        
        if (IAmMaster)
        {
            player.GetComponent<PlayerController>().SetTarget(target);
            
            // TODO: send game state to follower
        }
        else
        {
            // TODO: Send action to master
        }
    }

    public void OpponentAction_SetPlayerTarget(uint playerID, Vector2 target)
    {
        if (!IAmMaster)
        {
            Debug.LogError("Asked to process opponent action SetPlayerTarget not being a master");
            return;
        }
    }

    public void ApplyGameState(GameState state)
    {
        if (IAmMaster)
        {
            Debug.LogError("Asked to Apply game state being a master");
            return;
        }

        foreach (var playerState in state.PlayerStates)
        {
            Players[playerState.Id].GetComponent<PlayerController>().ApplyState(playerState);
        }
    }

    public GameState GetGameState()
    {
        GameState gameState = new GameState();
        foreach (var player in Players.Values)
        {
            gameState.PlayerStates.Add(player.GetComponent<PlayerController>().GetState());
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
