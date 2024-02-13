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
    private float rotationSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving;
    private float targetRotationAngle;
    private bool needsRotation;

    public void Initialize(bool isMy, byte id)
    {
        IsMy = isMy;
        ID = id;
    }
    void Start()
    {
        var body = transform.Find("Body");
        var outline = body.GetComponent<Outline>();
        outline.OutlineWidth = 0;
        outline.OutlineColor = PlayerConfig.OutlineColor;
        outline.OutlineMode = Outline.Mode.OutlineAll;

        if (IsMy)
        {
            body.GetComponent<Renderer>().material.color = PlayerConfig.MyColor;
        }
        else
        {
            body.GetComponent<Renderer>().material.color = PlayerConfig.OpponentColor;
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
            TargetY = targetPosition.z,
            RotationAngle = GetAngle()
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
        transform.rotation = Quaternion.Euler(0, state.RotationAngle * Mathf.Rad2Deg, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Move(Time.deltaTime);
        Rotate(Time.deltaTime);
    }

    public void SetMovementTarget(Vector2 target)
    {
        var target3D = new Vector3(target.x, PlayerConfig.Height, target.y);
        targetPosition = target3D;
        isMoving = true;
    }

    public void SetRotationTargetAngle(float angle)
    {
        targetRotationAngle = angle;
        needsRotation = true;
    }

    private void Rotate(float timeDelta)
    {
        if (isMoving)
        {
            TurnTowardsMovementTarget(timeDelta);
        }
        
        if (!needsRotation) {return;}
        
        TurnTowardsTargetRotationAngle(timeDelta);
    }
    
    public void Move(float timeDelta)
    {
        if (!isMoving) { return; }
        
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            AngleMovementSlowingCoefficient() * moveSpeed * timeDelta
            );
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
        }
    }

    public Vector2 GetPosition()
    {
        var position = transform.position;
        return new Vector2(position.x, position.z);
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = new Vector3(position.x, PlayerConfig.Height, position.y);
    }

    public float GetAngle()
    {
        return transform.eulerAngles.y * Mathf.Deg2Rad;
    }

    public void Stop()
    {
        isMoving = false;
    }
    
    void TurnTowardsMovementTarget(float deltaTime)
    {
        Vector3 targetDirection = targetPosition - transform.position;
        targetDirection.y = 0;
        
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, deltaTime * rotationSpeed);
    }

    private void TurnTowardsTargetRotationAngle(float deltaTime)
    {
        var targetQuaternion = Quaternion.Euler(0, Mathf.Rad2Deg * targetRotationAngle, 0);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetQuaternion,
            deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetQuaternion) < 1f)
        {
            needsRotation = false;
        }
    }
    
    float CalculateViewAngle(Vector3 point)
    {
        if ((point - transform.position).sqrMagnitude < 0.001f)
        {
            return 0f;
        }
        Vector3 directionToPoint = (point - transform.position).normalized;
        directionToPoint.y = 0;
        
        float dotProduct = Vector3.Dot(transform.forward.normalized, directionToPoint);
        float angleInRadians = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f));
        return angleInRadians;
    }

    float AngleMovementSlowingCoefficient()
    {
        var angle = CalculateViewAngle(targetPosition);
        return 0.25f + 0.75f * (1f - angle / Mathf.PI);
    }

    public void PlayGrabAnimation()
    {
        transform.Find("GrabCircle").GetComponent<Animator>().Play("GrabAnimation", -1, 0f);
    }

    public void PlayThroughAnimation()
    {
        transform.Find("ThroughCircle").GetComponent<Animator>().Play("ThroughAnimation", -1, 0f);

    }
}
