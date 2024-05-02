using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToMenu : MonoBehaviour
{
    public void SwitchToMenu()
    {
        GameObject.FindWithTag("SceneTransition").GetComponent<SceneTransition>().LoadScene("Menu");
    }
}
