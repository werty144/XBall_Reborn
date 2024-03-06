using UnityEngine;
using Random = System.Random;

public static class GoalRules
{
    private static Random random = new Random();
    
    public static bool GoalAttemptSuccess(BallController ball, GoalController goal)
    {
        var ballGoalVector = goal.transform.position - ball.transform.position;

        // var angle = Vector3.Angle(ball.GetComponent<Rigidbody>().velocity, ballGoalVector);
        // // In case detected after the ball has bounced
        // angle = Mathf.Min(angle, 180 - angle);
        // angle = Mathf.Clamp(angle, 0f, 90f);

        var speed = ball.GetComponent<Rigidbody>().velocity.magnitude;
        var probability = 1 / (1 + Mathf.Exp(0.6f * (speed - 4)));
        return random.NextDouble() < probability;
    }
}
