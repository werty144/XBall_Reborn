using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;


public struct PlayerConfig
{
    // The height of the center of the cylinder
    public static float Height = 1;
    public static float Radius = 0.5F;
    
    public static Color MyColor = new Color(100/255f, 149/255f, 237/255f);
    public static Color OpponentColor = new Color(205f/255f, 92f/255f, 92f/255f, 1f);
}

public class PlayerController : MonoBehaviour
{
    public bool IsMy;
    public uint ID;
    public ulong UserID;

    public BallController Ball { set; private get; }
    public bool isMoving { private set; get; }
    
    private Vector3 targetPosition;
    
    private float targetRotationAngle;
    private bool needsRotation;
    
    public void Colorize(Color color)
    {
        var body = transform.Find("Skin/Body");
        body.GetComponent<Renderer>().material.color = color;
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

    public void InterpolateToState(PlayerController targetState)
    {
        var interpolationFactor = 0.1f;
        SetMovementTarget(targetState.GetPosition());
        // transform.position = Vector3.Lerp(
        //     transform.position, 
        //     targetState.transform.position, 
        //     interpolationFactor);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetState.transform.rotation, 
            interpolationFactor);
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
        
        var viewAngle = CalculateViewAngle(targetPosition);

        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            MovementRules.AngleMovementSlowingCoefficient(viewAngle) * 
            MovementRules.BallOwningSlowingCoefficient(IsBallOwner()) * 
            MovementConfig.moveSpeed * 
            timeDelta
            );
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
        }
    }

    private bool IsBallOwner()
    {
        return Ball.Owned && Ball.Owner.ID == ID;
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
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, deltaTime * MovementConfig.rotationSpeed);
    }

    private void TurnTowardsTargetRotationAngle(float deltaTime)
    {
        var targetQuaternion = Quaternion.Euler(0, Mathf.Rad2Deg * targetRotationAngle, 0);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetQuaternion,
            deltaTime * MovementConfig.rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetQuaternion) < 1f)
        {
            needsRotation = false;
        }
    }
    
    public float CalculateViewAngle(Vector3 point)
    {
        if ((point - transform.position).sqrMagnitude < 0.001f)
        {
            return 0f;
        }
        Vector3 directionToPoint = (point - transform.position).normalized;
        directionToPoint.y = 0;
        
        float dotProduct = Vector3.Dot(transform.forward.normalized, directionToPoint);
        float angleInRadians = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f));
        return angleInRadians * Mathf.Rad2Deg;
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
