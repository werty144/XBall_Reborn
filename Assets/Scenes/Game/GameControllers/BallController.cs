using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private PlayerController Owner;
    private bool Owned = false;
    
    public BallState GetState()
    {
        return new BallState
        {
            Position = ToVector3ProtoBuf(transform.position),
            AngularVelocity = ToVector3ProtoBuf(GetComponent<Rigidbody>().angularVelocity),
            Velocity = ToVector3ProtoBuf(GetComponent<Rigidbody>().velocity)
        };
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
            transform.position = new Vector3(ownerPosition.x, PlayerConfig.Height * 2 + GameConfig.SphereRadius,
                ownerPosition.y);
        }
    }
}
