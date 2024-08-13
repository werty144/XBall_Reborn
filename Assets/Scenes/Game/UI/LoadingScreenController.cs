using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    public GameObject ToMenuButton;
    private void OnEnable()
    {
        ToMenuButton.SetActive(false);
        Invoke(nameof(ShowToMenuButton), 7f * Time.timeScale);
    }

    private void ShowToMenuButton()
    {
        ToMenuButton.SetActive(true);
    }

    public void OnToMenuButton()
    {
        GameObject.FindWithTag("SceneTransition").GetComponent<SceneTransition>().LoadScene("Menu");
    }
}
