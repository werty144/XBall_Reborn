using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManagerCreator : MonoBehaviour
{
    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;

        if (setupInfo.IAmMaster)
        {
            gameObject.AddComponent<ConnectionManagerMaster>();
        }
        else
        {
            gameObject.AddComponent<ConnectionManagerFollower>();
        }
        
    }
}
