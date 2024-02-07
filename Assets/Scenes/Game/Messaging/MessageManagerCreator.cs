using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageManagerCreator : MonoBehaviour
{
    private void Awake()
    {
        var global = GameObject.FindWithTag("Global");
        var setupInfo = global.GetComponent<GameStarter>().Info;

        if (global.GetComponent<GameStarter>().IsTest)
        {
            gameObject.AddComponent<MessageManagerTest>();
            return;
        }

        if (setupInfo.IAmMaster)
        {
            gameObject.AddComponent<MessageManagerMaster>();
        }
        else
        {
            gameObject.AddComponent<MessageManagerFollower>();
        }
        
    }
}
