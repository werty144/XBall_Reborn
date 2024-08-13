using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionScheduler : MonoBehaviour
{
    private uint nextID = 0;
    private uint nextToExecute = 0;
    
    public void Schedule(Action action, int delayMillis)
    {
        StartCoroutine(DelayedAction(action, delayMillis, nextID));
        nextID++;
    }

    IEnumerator DelayedAction(Action action, int delayMillis, uint id)
    {
        yield return new WaitForSeconds(0.001f * delayMillis);
        while (nextToExecute != id)
        {
            yield return null;
        }

        action();
        nextToExecute++;
    }
}
