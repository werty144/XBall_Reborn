using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InitialState
{
    public static GameState GetInitialState(int n)
    {
        var gameState = new GameState();
        byte spareID = 0;
        float masterZ = -GameConfig.FieldLength / 4;
        float followerZ = GameConfig.FieldLength / 4;
        for (int i = 0; i < n; i++)
        {
            var x = GameConfig.FieldWidth * (i + 1) / (n + 1) - GameConfig.FieldWidth / 2;

            var masterPlayer = new PlayerState
            {
                Id = spareID,
                X = x,
                Y = masterZ,
                IsMoving = false,
                RotationAngle = 0
            };
            gameState.PlayerStates.Add(masterPlayer);
            spareID++;
            
            var followerPlayer = new PlayerState
            {
                Id = spareID,
                X = x,
                Y = followerZ,
                IsMoving = false,
                RotationAngle = Mathf.PI
            };
            gameState.PlayerStates.Add(followerPlayer);
            spareID++;
        }

        gameState.BallState = new BallState
        {
            Position = new Vector3ProtoBuf { X = 0f, Y = GameConfig.BallRadius, Z = 0f },
            Velocity = new Vector3ProtoBuf(),
            AngularVelocity = new Vector3ProtoBuf()
        };
        return gameState;
    }
}
