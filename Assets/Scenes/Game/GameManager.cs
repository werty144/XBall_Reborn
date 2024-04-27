using System.Collections;
using System.Collections.Generic;
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
    
    private MessageManager MessageManager;
    private UIManagerGame UIManager;

    private GamePhase GamePhase;
    void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
        UIManager = GameObject.FindWithTag("UIManager").GetComponent<UIManagerGame>();

        GamePhase = GamePhase.BeforeGame;
        
        UIManager.DisplayLoading();
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

    public void OnConnectionUnknownProblem()
    {
        
    }
}
