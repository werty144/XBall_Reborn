using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application = UnityEngine.Device.Application;

public class ExitScreenManager : MonoBehaviour
{
    public UIManagerMenu UIManagerMenu;
    
    public void OnExitButtonExitScreen()
    {
        Application.Quit();
    }

    public void OnReturnButton()
    {
        UIManagerMenu.OnReturnFromExitScreen();
    }
}
