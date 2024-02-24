using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pb = global::Google.Protobuf;

public class ProtobufUtils : MonoBehaviour
{
    public static Vector3ProtoBuf ToVector3ProtoBuf(Vector3 vector3)
    {
        return new Vector3ProtoBuf
        {
            X = vector3.x,
            Y = vector3.y,
            Z = vector3.z
        };
    }

    public static Vector3 FromVector3Protobuf(Vector3ProtoBuf vector3ProtoBuf)
    {
        return new Vector3
        {
            x = vector3ProtoBuf.X,
            y = vector3ProtoBuf.Y,
            z = vector3ProtoBuf.Z
        };
    }
}
