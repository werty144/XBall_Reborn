using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;

    void Awake()
    {
        if (instance == null)
        {
            // If instance doesn't exist, set it to this object and make it persistent
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // If instance already exists and it's not this, then destroy this object to enforce the singleton
            Destroy(gameObject);
        }
    }
}
