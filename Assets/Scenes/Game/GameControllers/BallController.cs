using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IngameDebugConsole;
using UnityEngine;
using UnityEngine.PlayerLoop;

public struct BallConfig
{
    public static Color CanGrabColor = new Color(102f/255f, 204f/255f, 153f/255f);
}

public class BallController : MonoBehaviour
{
    public PlayerController Owner { get; private set; }
    public bool Owned { get; private set; }

    private Outline Outline;
    private float OutlineWidth = 3f;
    private Client Client;

    private void Start()
    {
        Outline = transform.Find("Sphere").GetComponent<Outline>();
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
    }

    public BallState GetState()
    {
        return new BallState
        {
            Position = ProtobufUtils.ToVector3ProtoBuf(transform.position),
            AngularVelocity = ProtobufUtils.ToVector3ProtoBuf(GetComponent<Rigidbody>().angularVelocity),
            Velocity = ProtobufUtils.ToVector3ProtoBuf(GetComponent<Rigidbody>().velocity),
            IsOwned = Owned,
            OwnerId = Owned ? Owner.ID : 0
        };
    }

    public void ApplyState(BallState state)
    {
        transform.position = ProtobufUtils.FromVector3Protobuf(state.Position);
        GetComponent<Rigidbody>().angularVelocity = ProtobufUtils.FromVector3Protobuf(state.AngularVelocity);
        GetComponent<Rigidbody>().velocity = ProtobufUtils.FromVector3Protobuf(state.Velocity);
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
            interpolationFactor / 2);

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
        ManageOutline();
        if (Owned)
        {
            var ownerPosition = Owner.GetPosition();
            var targetPosition = new Vector3(ownerPosition.x, PlayerConfig.Height * 2 + GameConfig.BallRadius,
                ownerPosition.y);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 20f);
        }
    }

    private void ManageOutline()
    {
        var existsMyNotOwnerWhoCanGrab = false;
        foreach (var player in from player in Client.GetPlayers().Values where player.IsMy where !Owned || Owner.ID != player.ID where ActionRules.IsValidGrab(player.transform, transform) select player)
        {
            existsMyNotOwnerWhoCanGrab = true;
        }
        
        if (existsMyNotOwnerWhoCanGrab)
        {
            Outline.OutlineWidth = OutlineWidth;
            Outline.OutlineMode = Outline.Mode.OutlineAll;
            Outline.OutlineColor = BallConfig.CanGrabColor;
        }
        else
        {
            if (Owned)
            {
                Outline.OutlineWidth = 0;
            }
            else
            {
                Outline.OutlineWidth = OutlineWidth;
                Outline.OutlineMode = Outline.Mode.OutlineHidden;
                Outline.OutlineColor = Color.white;
            }
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
