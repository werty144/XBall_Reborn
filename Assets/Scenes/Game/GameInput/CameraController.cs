using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float fieldLength;
    private float movementSpeed = 10f;

    void Start()
    {
        var defaultPlaneLength = 10;
        var floor = GameObject.FindWithTag("Floor");
        var scale = floor.GetComponent<Transform>().localScale;
        fieldLength = scale.z * defaultPlaneLength;
        
        if (!GameObject.FindWithTag("Global").GetComponent<GameStarter>().Info.IAmMaster)
        {
            movementSpeed *= -1;
            
            var curCamPos = transform.position;
            var newCameraPosition = new Vector3(
                curCamPos.x,
                curCamPos.y,
                -curCamPos.z);
            transform.position = newCameraPosition;
        
            var curCamRot = transform.rotation.eulerAngles;
            var newCamRot = new Vector3(
                curCamRot.x,
                180,
                curCamRot.z);
            transform.rotation = new Quaternion {eulerAngles = newCamRot};
        }
    }

    void Update()
    {
        float translation = 0;

        // Check if the mouse is in the upper part of the screen
        if (Input.mousePosition.y > Screen.height * 0.9)
        {
            translation = movementSpeed * Time.unscaledDeltaTime;
        }
        else if (Input.mousePosition.y < Screen.height * 0.1)
        {
            translation = -movementSpeed * Time.unscaledDeltaTime;
        }

        float newZ = Mathf.Clamp(transform.position.z + translation, -fieldLength, fieldLength);

        transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
    }
}
