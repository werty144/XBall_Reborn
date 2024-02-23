using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private PlayerController Owner;
    public bool Owned { get; private set; } = false ;

public BallState GetState()
    {
        return new BallState
        {
            Position = ToVector3ProtoBuf(transform.position),
            AngularVelocity = ToVector3ProtoBuf(GetComponent<Rigidbody>().angularVelocity),
            Velocity = ToVector3ProtoBuf(GetComponent<Rigidbody>().velocity),
            IsOwned = Owned,
            OwnerId = Owned ? Owner.ID : 0
        };
    }

    public void ApplyState(BallState state)
    {
        transform.position = FromVector3Protobuf(state.Position);
        GetComponent<Rigidbody>().angularVelocity = FromVector3Protobuf(state.AngularVelocity);
        GetComponent<Rigidbody>().velocity = FromVector3Protobuf(state.Velocity);
        Owned = state.IsOwned;
        if (Owned)
        {
            Owner = GameObject.FindWithTag("Client").GetComponent<Client>().GetPlayers()[state.OwnerId];
        }
    }

    public void InterpolateToState(BallController targetState)
    {
        var interpolationFactor = 0.1f;
        transform.position = Vector3.Lerp(
            transform.position, 
            targetState.transform.position, 
            interpolationFactor);

        GetComponent<Rigidbody>().velocity = Vector3.Lerp(
            GetComponent<Rigidbody>().velocity, 
            targetState.GetComponent<Rigidbody>().velocity, interpolationFactor);

        GetComponent<Rigidbody>().angularVelocity = Vector3.Lerp(
            GetComponent<Rigidbody>().angularVelocity, 
            targetState.GetComponent<Rigidbody>().angularVelocity, 
            interpolationFactor);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetState.transform.rotation, 
            interpolationFactor);
    }

    Vector3ProtoBuf ToVector3ProtoBuf(Vector3 vector3)
    {
        return new Vector3ProtoBuf
        {
            X = vector3.x,
            Y = vector3.y,
            Z = vector3.z
        };
    }

    Vector3 FromVector3Protobuf(Vector3ProtoBuf vector3ProtoBuf)
    {
        return new Vector3
        {
            x = vector3ProtoBuf.X,
            y = vector3ProtoBuf.Y,
            z = vector3ProtoBuf.Z
        };
    }

    public void SetOwner(PlayerController owner)
    {
        Owned = true;
        Owner = owner;
    }

    public void RemoveOwner()
    {
        Owned = false;
    }

    private void Update()
    {
        if (Owned)
        {
            var ownerPosition = Owner.GetPosition();
            var targetPosition = new Vector3(ownerPosition.x, PlayerConfig.Height * 2 + GameConfig.BallRadius,
                ownerPosition.y);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 20f);
        }
    }

    public void ThrowTo(Vector3 target)
    {
        Owned = false;

        var ballPosition = transform.position;
        Vector3 ballToTarget = target - ballPosition;
        if (ballToTarget.magnitude < 0.1)
        {
            return;
        }
        Vector3 ballToTargetXZ = new Vector3(ballToTarget.x, 0, ballToTarget.z);
        float dot = Vector3.Dot(ballToTarget.normalized, Vector3.up);
        float angleRadians = Mathf.Acos(dot);
        float launchAngleRadians = (Mathf.PI - angleRadians) * 0.5f;
        float horizontalDistance = ballToTargetXZ.magnitude;
        float heightDifference = target.y - ballPosition.y;

        if (horizontalDistance < 0.1f)
        {
            float velocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * (heightDifference + 0.5f));
            GetComponent<Rigidbody>().velocity = ballToTarget.normalized * velocity;
            return;
        }
        
        float gravity = Physics.gravity.magnitude;
        float velocityMagnitude = Mathf.Sqrt((gravity * horizontalDistance * horizontalDistance) / 
                                             (2 * Mathf.Cos(launchAngleRadians) * Mathf.Cos(launchAngleRadians) * (horizontalDistance * Mathf.Tan(launchAngleRadians) - heightDifference)));

        Vector3 toTargetXZ = ballToTargetXZ.normalized;
        var launchVelocity = toTargetXZ * Mathf.Cos(launchAngleRadians) * velocityMagnitude + Vector3.up * Mathf.Sin(launchAngleRadians) * velocityMagnitude;
        GetComponent<Rigidbody>().velocity = launchVelocity;
    }
}
