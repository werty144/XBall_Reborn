using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = System.Random;

public struct ActionRulesConfig
{
    public static float GrabRadius = 2f;
    public static float GrabAngle = 120;

    public static int GrabDuration = 200; // millis
    public static int ThrowDuration = GrabDuration;

    public static float GrabCooldown = 1f; // secs
}

public static class ActionRules
{
    private static Random random = new Random();
    public static bool IsValidGrab(Transform player, Transform ball)
    {
        if (Vector3.Distance(player.position, ball.position) > ActionRulesConfig.GrabRadius)
        {
            return false;
        }

        Vector3 playerToBall = ball.position - player.position;
        Vector3 playerToBallHorizontal = new Vector3(playerToBall.x, 0f, playerToBall.z);
        Vector3 playerForwardHorizontal = new Vector3(player.forward.x, 0f, player.forward.z);
        float angle = Vector3.Angle(playerForwardHorizontal, playerToBallHorizontal);
        if (angle > ActionRulesConfig.GrabAngle)
        {
            return false;
        }

        return true;
    }

    public static Vector3 CalculateThrowTarget(PlayerController player, Vector3 initialTarget)
    {
        var viewAngle = player.CalculateViewAngle(initialTarget);
        var distance = Vector3.Distance(player.transform.position, initialTarget);
        var distanceVariation = new Vector3(
            ThrowDistanceDeviation(distance),
            ThrowDistanceDeviation(distance),
            ThrowDistanceDeviation(distance)
        );
        var angleVariation = new Vector3(
            ThrowAngleDeviation(viewAngle),
            ThrowAngleDeviation(viewAngle),
            ThrowAngleDeviation(viewAngle)
        );

        return initialTarget + distanceVariation + angleVariation;
    }

    private static float ThrowDistanceDeviation(float distance)
    {

        var variance = distance / 15;
        var distanceDeviation = (float)(random.NextDouble() * 2 * variance - variance);
        return distanceDeviation;
    }
    
    private static float ThrowAngleDeviation(float angle)
    {
        if (angle <= 90)
        {
            return 0;
        }

        return 2 * (angle - 90) / 90;
    }

    public static bool BallGrabSuccess(PlayerController player, BallController ball)
    {
        if (!IsValidGrab(player.transform, ball.transform)) { return false; }

        if (!ball.Owned || ball.Owner.IsMy)
        {
            return true;
        }
        var uniformValue = random.NextDouble();
        return  uniformValue > 0.5;
    }
}
