using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private GameManager GameManager;

    private void OnEnable()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    public void OnMenuButton()
    {
        GameManager.GameEnd();
        SceneManager.LoadScene("Menu");
    }
}
