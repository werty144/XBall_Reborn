using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlacer : MonoBehaviour
{

    public float z16_9;
    public float z5_4;
    void Start()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        
        float aspect16_9 = 16f / 9f;
        float aspect5_4 = 5f / 4f;

        Vector3 cameraPosition = Camera.main.transform.position;

        if (aspectRatio >= aspect16_9)
        {
            cameraPosition.z = z16_9;
        }
        else if (aspectRatio <= aspect5_4)
        {
            cameraPosition.z = z5_4;
        }
        else
        {
            float t = (aspectRatio - aspect5_4) / (aspect16_9 - aspect5_4);
            cameraPosition.z = Mathf.Lerp(z5_4, z16_9, t);
        }

        Camera.main.transform.position = cameraPosition;
    }
}
