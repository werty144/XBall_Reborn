using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2PCreator : MonoBehaviour
{
    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;

        if (global.GetComponent<GameStarter>().IsTest)
        {
            gameObject.AddComponent<P2PTestMaster>();
            return;
        }

        if (setupInfo.IAmMaster)
        {
           gameObject.AddComponent<P2PMaster>();
        }
        else
        {
            gameObject.AddComponent<P2PFollower>();
        }
        
    }
}
