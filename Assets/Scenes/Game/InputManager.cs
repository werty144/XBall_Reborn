using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GameManager GameManager;
    private Transform CameraTransform;
    
    private GameObject selectedPlayer;

    private void Start()
    {
        GameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        CameraTransform = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
        
        if (!GameManager.IsMaster())
        {
            var curCamPos = CameraTransform.position;
            var newCameraPosition = new Vector3(
                curCamPos.x,
                curCamPos.y,
                -curCamPos.z);
            CameraTransform.position = newCameraPosition;

            var curCamRot = CameraTransform.rotation.eulerAngles;
            var newCamRot = new Vector3(
                curCamRot.x,
                180,
                curCamRot.z);
            CameraTransform.rotation = new Quaternion {eulerAngles = newCamRot};
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    DeselectPlayer();
                    SelectPlayer(hit.collider.gameObject);
                }
                if (hit.collider.gameObject.CompareTag("Floor"))
                {
                    DeselectPlayer();
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && selectedPlayer != null) // Right click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var vectorPlane = new Vector2(hit.point.x, hit.point.z); 
                GameManager.InputAction_SetPlayerTarget(selectedPlayer, vectorPlane);
            }
        }
    }

    void DeselectPlayer()
    {
        if (selectedPlayer == null) { return; }

        selectedPlayer.GetComponent<Outline>().OutlineWidth = 0;
        selectedPlayer = null;
    }

    void SelectPlayer(GameObject selected)
    {
        selectedPlayer = selected;
        selectedPlayer.GetComponent<Outline>().OutlineWidth = PlayerConfig.OutlineWidth;
    }
}
