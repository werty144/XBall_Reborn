using UnityEngine;
using Random = System.Random;

public static class GoalRules
{
    private static Random random = new Random();
    
    public static bool GoalAttemptSuccess(BallController ball, GoalController goal)
    {
        var ballGoalVector = goal.transform.position - ball.transform.position;

        var angle = Vector3.Angle(ball.GetComponent<Rigidbody>().velocity, ballGoalVector);
        // In case detected after the ball has bounced
        angle = Mathf.Min(angle, 180 - angle);
        Debug.Log(angle);
        angle = Mathf.Clamp(angle, 0f, 90f);
        var probability = (90 - angle) / 90;
        return random.NextDouble() < probability;
    }
}
