public struct MovementConfig
{
    public static float moveSpeed = 5f;
    public static float rotationSpeed = 5f;
}

public static class MovementRules
{
    public static float AngleMovementSlowingCoefficient(float angle)
    {
        return 0.25f + 0.75f * (1f - angle / 180);
    }

    public static float BallOwningSlowingCoefficient(bool isOwner)
    {
        return isOwner ? 0.8f : 1f;
    }
}
