using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitManager : MonoBehaviour
{
    public Storage Storage;
    public ProfileManager ProfileManager;
    void Start()
    {
        Storage.Initialize();
        ProfileManager.Initialize();
    }
}
