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
    }

    public void OnConnectionRemoteProblem()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayPeerDropped();
    }

    public void OnConnectionLocalProblem()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayLocalProblem();
    }

    public void OnConnectionPeerDisconnected()
    {
        GamePhase = GamePhase.Pause;
        UIManager.DisplayPeerDropped();
    }

    public void ResumeGame()
    {
        Assert.AreEqual(GamePhase.Pause, GamePhase);
        GamePhase = GamePhase.InGame;
        UIManager.RemoveScreen();
    }

    public void OnConnectionUnknownProblem()
    {
        
    }
}
