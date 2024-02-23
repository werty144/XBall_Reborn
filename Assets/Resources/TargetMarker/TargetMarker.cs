using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMarker : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(SelfDestroy), 3f);
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
