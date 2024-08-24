using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public GameObject WelcomeTask;
    public GameObject SelectPlayerTask;
    public GameObject MovePlayerTask;
    public GameObject CameraTask;
    public GameObject GrabTheBallTask;
    public GameObject GivePassTask;
    public GameObject StealTheBallTask;
    public GameObject GoalTask;
    public GameObject FinalTask;

    private List<GameObject> Tasks;
    private int currentTask;

    private void Awake()
    {
        Tasks = new List<GameObject>
        {
            WelcomeTask, 
            SelectPlayerTask,
            MovePlayerTask,
            CameraTask,
            GrabTheBallTask,
            GivePassTask,
            StealTheBallTask,
            GoalTask,
            FinalTask
        };
    }

    private void Start()
    {
        Tasks[currentTask].SetActive(true);
    }

    public void Done()
    {
        Tasks[currentTask].SetActive(false);
        currentTask++;
        if (currentTask == Tasks.Count)
        {
            GameObject.FindWithTag("SceneTransition").GetComponent<SceneTransition>().LoadScene("Menu");
            return;
        }
        Tasks[currentTask].SetActive(true);
    }
}
