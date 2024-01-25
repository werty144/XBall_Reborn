using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    void Start()
    {
        loadingScreen.SetActive(true);
    }

    public void SwitchToGame()
    {
        loadingScreen.SetActive(false);
    }
}
