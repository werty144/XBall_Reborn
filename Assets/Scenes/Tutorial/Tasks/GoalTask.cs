using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoalTask : MonoBehaviour
{
    // z = 6
    public TaskManager TaskManager;
    public InputManager InputManager;
    public CameraController CameraController;
    public TextMeshProUGUI mainText;
    public GameObject anyKey;
    public GameObject ProbabiltyIndicatorPointer;
    public ClientTutorial Client;
    
    private int step = 1;

    private void OnEnable()
    {
        InputManager.enabled = false;
        CameraController.enabled = false;
        mainText.text = "";
    }

    private void Update()
    {
        switch (step)
        {
            case 1:
                if (CameraController.transform.position.z < 6f)
                {
                    CameraController.transform.position += new Vector3(0, 0, 15f * Time.deltaTime);
                }
                else
                {
                    step++;
                }
                break;
            case 2:
                CameraController.enabled = true;
                mainText.text = "This is a goal and an indicator.";
                anyKey.SetActive(true);
                ProbabiltyIndicatorPointer.SetActive(true);
                step++;
                break;
            case 3:
                if (Input.anyKeyDown)
                {
                    mainText.text = "An indicator shows how likely you are to score. It's easy to score when close and without defenders around.";
                    step++;
                }
                break;
            case 4:
                if (Input.anyKeyDown)
                {
                    mainText.text = "To throw a ball to a goal press Q twice.\nGo score!";
                    anyKey.SetActive(false);
                    ProbabiltyIndicatorPointer.SetActive(false);
                    InputManager.enabled = true;
                    step++;
                }
                break;
            case 5:
                if (Client.scored)
                {
                    TaskManager.Done();
                }
                break;
        }
    }
}
