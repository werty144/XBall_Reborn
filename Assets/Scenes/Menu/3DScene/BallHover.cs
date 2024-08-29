using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHover : MonoBehaviour
{
    public Outline outline;
    public UIManagerMenu UIManagerMenu;
    
    private void OnMouseOver()
    {
        if (UIManagerMenu.IsInitialView())
        {
            outline.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        outline.enabled = false;
    }

    private void OnMouseDown()
    {
        if (UIManagerMenu.IsInitialView())
        {
            UIManagerMenu.OnEasterEggEnter();
        }
    }
}
