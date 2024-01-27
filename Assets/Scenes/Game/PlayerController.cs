using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;


public struct PlayerConfig
{
    // The height of the center of the cylinder
    public static float Height = 1;
    public static float Radius = 0.5F;

    public static float OutlineWidth = 4;

    public static Color MyColor = new Color(100/255f, 149/255f, 237/255f);
    public static Color OpponentColor = new Color(205f/255f, 92f/255f, 92f/255f, 1f);
    public static Color OutlineColor = new Color(100f/255f, 239f/255f, 213f/255f, 1f);
}

public class PlayerController : MonoBehaviour
{
    public bool IsMy;
    public uint ID;
    
    private float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    public void Initialize(bool isMy, byte id)
    {
        IsMy = isMy;
        ID = id;
    }
    void Start()
    {
        GetComponent<Outline>().OutlineWidth = 0;
        GetComponent<Outline>().OutlineColor = PlayerConfig.OutlineColor;
        GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;

        if (IsMy)
        {
            GetComponent<Renderer>().material.color = PlayerConfig.MyColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = PlayerConfig.OpponentColor;
        }
    }

    public PlayerState GetState()
    {
        var position = transform.position;
        return new PlayerState
        {
            Id = ID,
            X = position.x,
            Y = position.z,
            IsMoving = isMoving,
            TargetX = targetPosition.x,
            TargetY = targetPosition.z
        };
    }

    public void ApplyState(PlayerState state)
    {
        if (state.Id != ID)
        {
            Debug.LogWarning("Applying state to another player");
            return;
        }

        transform.position = new Vector3(state.X, PlayerConfig.Height, state.Y);
        isMoving = state.IsMoving;
        targetPosition = new Vector3(state.TargetX, PlayerConfig.Height, state.TargetY);
    }

    // Update is called once per frame
    void Update()
    {
        Move(Time.deltaTime);
    }

    public void SetTarget(Vector2 target)
    {
        var target3D = new Vector3(target.x, PlayerConfig.Height, target.y);
        targetPosition = target3D;
        isMoving = true;
    }
    
    public void Move(float timeDelta)
    {
        if (!isMoving) { return; }
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * timeDelta);
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
        }
    }
}
