using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCreator : MonoBehaviour
{
    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;

        if (setupInfo.IAmMaster)
        {
            gameObject.AddComponent<Server>();
        }
    }
}
