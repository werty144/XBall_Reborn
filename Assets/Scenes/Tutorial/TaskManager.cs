using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public GameObject WelcomeTask;
    public GameObject SelectPlayerTask;

    private List<GameObject> Tasks;
    private int currentTask;

    private void Awake()
    {
        Tasks = new List<GameObject>
        {
            WelcomeTask, 
            SelectPlayerTask
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
