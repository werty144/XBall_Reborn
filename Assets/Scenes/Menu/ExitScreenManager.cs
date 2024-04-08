using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application = UnityEngine.Device.Application;

public class ExitScreenManager : MonoBehaviour
{

    public GameObject ExitScreen;
    
    public void OnExitButtonMenu()
    {
        ExitScreen.SetActive(true);
    }

    public void OnExitButtonExitScreen()
    {
        Application.Quit();
    }

    public void OnReturnButton()
    {
        ExitScreen.SetActive(false);
    }
}
