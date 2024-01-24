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
    
    private GameObject[] Players;
    private CSteamID OpponentID;
    private bool IAmMaster;
    private FieldParams FieldParams;

    public void Setup(SetupInfo setupInfo)
    {
        OpponentID = setupInfo.OpponentID;
        IAmMaster = setupInfo.IAmMaster;
        
        SetFieldParams();

        if (IAmMaster)
        {
            CreatePlayers(setupInfo.NumberOfPlayers);
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
        uint spareID = 0;
        float myZ = -FieldParams.Length / 4;
        float opponentZ = FieldParams.Length / 4;
        for (int i = 0; i < n; i++)
        {
            var x = FieldParams.Width * (i + 1) / (n + 1) - FieldParams.Width / 2;
            
            var myPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerParams.Height, myZ), Quaternion.identity);
            myPlayer.GetComponent<PlayerController>().Initialize(true, spareID);
            spareID++;
            
            var opponentPlayer = Instantiate(PlayerPrefab, 
                new Vector3(x, PlayerParams.Height, opponentZ), Quaternion.identity);
            opponentPlayer.GetComponent<PlayerController>().Initialize(false, spareID);
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
            
            // TODO: send position to follower
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

    public void ApplyPosition(int position)
    {
        if (IAmMaster)
        {
            Debug.LogError("Asked to Apply position being a master");
            return;
        }
    }
}
