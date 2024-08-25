using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public TextMeshProUGUI mainText;
    public GameObject anyKey;

    private bool ready;

    private void OnEnable()
    {
        mainText.text = "Game ends at a 3-point difference.\nEnjoy XBall!";
        Invoke(nameof(Ready), 2f);
    }

    void Ready()
    {
        anyKey.SetActive(true);
        anyKey.GetComponent<TextMeshProUGUI>().text = "Press Space to quit";
        ready = true;
    }

    private void Update()
    {
        if (!ready) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TaskManager.Done();
        }
    }
}
