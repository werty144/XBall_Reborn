using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Assertions;

enum GamePhase
{
    BeforeGame = 0,
    InGame = 1,
    Pause = 2,
    AfterGame = 3
}

public class GameManager : MonoBehaviour
{
    public ScorePanelController ScorePanelController;
    public InputManager InputManager;
    
    private MessageManager MessageManager;
    private UIManagerGame UIManager;

    private GamePhase GamePhase;
    void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
        UIManager = GameObject.FindWithTag("UIManager").GetComponent<UIManagerGame>();

        GamePhase = GamePhase.BeforeGame;
        
        UIManager.DisplayLoading();
        InputManager.enabled = false;
    }

    public void OnConnectedToServer()
    {
        switch (GamePhase)
        {
            case GamePhase.BeforeGame:
                MessageManager.SendReady();
                break;
        }
    }
    
    public void OnGameStart()
    {
        GamePhase = GamePhase.InGame;
        InputManager.enabled = true;
        UIManager.RemoveScreen();
        ScorePanelController.StartTimer();
    }

    public void OnConnectionRemoteProblem()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayPeerDropped();
        ScorePanelController.PauseTimer();
    }

    public void OnConnectionLocalProblem()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayLocalProblem();
        ScorePanelController.PauseTimer();
    }

    public void OnConnectionPeerDisconnected()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayPeerDropped();
        ScorePanelController.PauseTimer();
    }

    public void ResumeGame()
    {
        Assert.AreEqual(GamePhase.Pause, GamePhase);
        GamePhase = GamePhase.InGame;
        UIManager.RemoveScreen();
        ScorePanelController.ResumeTimer();
    }
    
    public void GameEnd(GameEnd gameEnd)
    {
        InputManager.enabled = false;
        
        var gameEnder = GameObject.FindWithTag("Global").GetComponent<GameEnder>();
        gameEnder.Score = gameEnd.Score.ToDictionary(x => x.Key, x => x.Value);
        gameEnder.Winner = new CSteamID(gameEnd.Winner);

        var storage = GameObject.FindWithTag("Global").GetComponent<Storage>();
        var currentStats = storage.GetUserData();
        currentStats.gamesPlayed++;
        if (gameEnder.Winner == Steam.MySteamID())
        {
            currentStats.wins++;
        }
        storage.SaveData(currentStats);
        
        Invoke(nameof(SwitchToGameEnd), 3f * Time.timeScale);
    }

    void SwitchToGameEnd()
    {
        GameObject.FindWithTag("SceneTransition").GetComponent<SceneTransition>().LoadScene("GameEnd");
    }

    public void OnConnectionUnknownProblem()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayUnknownProblem();
        ScorePanelController.PauseTimer();
    }
}
