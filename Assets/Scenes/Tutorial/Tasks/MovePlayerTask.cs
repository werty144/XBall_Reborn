using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MovePlayerTask : MonoBehaviour
{
    public TaskManager TaskManager;
    public GameObject MyPlayer;
    public TextMeshProUGUI mainText;
    public GameObject pointer;
    
    private void OnEnable()
    {
        mainText.text = "Right click to move the selected player.\n Move your player to the pointer.";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            mainText.enableAutoSizing = false;
            mainText.text = "You can press S to stop.";
        }

        var playerPos = MyPlayer.transform.position;
        playerPos.y = 0;
        var pointerPos = pointer.transform.position;
        pointerPos.y = 0;

        if (Vector3.Distance(playerPos, pointerPos) < 1f)
        {
            TaskManager.Done();
        }
    }

    private void OnDisable()
    {
        mainText.enableAutoSizing = true;
    }
}
