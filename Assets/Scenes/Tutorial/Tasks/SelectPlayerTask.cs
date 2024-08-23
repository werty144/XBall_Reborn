using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class SelectPlayerTask : MonoBehaviour
{
    public ClientTutorial client;
    public InputManager inputManager;
    public PlayerSelection playerSelection;

    public TextMeshProUGUI mainText;
    public TaskManager taskManager;

    public GameObject MyPlayer;

    private void OnEnable()
    {
        mainText.text = "Here is your player.\nSelect it by left clicking or pressing 1 (its number).";
        MyPlayer.SetActive(true);
        client.AddMyPlayer(MyPlayer);
        inputManager.enabled = true;
        playerSelection.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerSelection.GetSelected() == MyPlayer)
        {
            taskManager.Done();
        }
    }
}
