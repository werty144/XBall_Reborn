using System.Collections.Generic;

public interface StateHolder
{
    public GameState GetGameState();
    public Dictionary<uint, PlayerController> GetPlayers();
    public void ApplyGameState(GameState state);
}
