using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public static class GoalRules
{
    private const float distCoef = -0.0667f;
    private static Random random = new Random();
    
    public static float GoalAttemptSuccessProbability(
        Dictionary<uint, PlayerController> players, 
        PlayerController thrower, 
        BallController ball, 
        GameObject goal)
    {
        var goalPosition = goal.transform.position;
        var ballGoal = goalPosition - ball.transform.position;
        ballGoal.y = 0;
        var distance = ballGoal.magnitude;

        PlayerController nearestOpponent = null;
        float minDist = Mathf.Infinity;
        foreach (var player in players.Values)
        {
            if (player.UserID == thrower.UserID)
            {
                continue;
            }

            var dist = Vector2.Distance(player.GetPosition(), thrower.GetPosition());
            if (dist < minDist)
            {
                minDist = dist;
                nearestOpponent = player;
            }
        }

        float opponentCoef;
        if (minDist > 2)
        {
            opponentCoef = 1;
        }
        else
        {
            opponentCoef = thrower.CalculateViewAngle(nearestOpponent!.transform.position) > 90 ? 0.8f : 0.5f;
        }

        var probability = Mathf.Max(distCoef * distance + 1, 0.05f) * opponentCoef;
        return probability;
    }

    public static bool GoalAttemptSuccess(
        Dictionary<uint, PlayerController> players,
        PlayerController thrower,
        BallController ball,
        GameObject goal)
    {
        var probability = GoalAttemptSuccessProbability(players, thrower, ball, goal);
        return random.NextDouble() < probability;
    }
}
