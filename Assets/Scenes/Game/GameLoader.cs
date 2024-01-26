using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public GameObject loadingScreen;

    private bool gameStarted = false;
    void Start()
    {
        if (gameStarted) { return; }
        loadingScreen.SetActive(true);
    }

    public void SwitchToGame()
    {
        gameStarted = true;
        loadingScreen.SetActive(false);
    }
}
