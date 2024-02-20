using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ActionRulesConfig
{
    public static float GrabRadius = 2f;
    public static float GrabAngle = 120;
}

public static class ActionRules
{
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
}
