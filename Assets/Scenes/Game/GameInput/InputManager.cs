using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Client Client;
    
    private PlayerSelection PlayerSelection;

    private int layerMask;

    private bool ThrowIntention = false;

    private void Start()
    {
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
        PlayerSelection = gameObject.GetComponent<PlayerSelection>();
        
        layerMask = (1 << LayerMask.NameToLayer("Server")) +
                        (1 << LayerMask.NameToLayer("Dummy")) + 
                        (1 << LayerMask.NameToLayer("ClientServer")) + 
                        (1 << LayerMask.NameToLayer("DummyServer"));
        layerMask = ~layerMask;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessLeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ProcessRightClick();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ProcessStop();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ProcessGrab();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ProcessThroughIntention();
        }
    }

    void ProcessThroughIntention()
    {
        if (ThrowIntention)
        {
            var selectedPlayer = PlayerSelection.GetSelected();
            Client.GaolShotInput(selectedPlayer.GetComponent<PlayerController>().ID);
            ThrowIntention = false;
        }
        else
        {
            ThrowIntention = true;
        }
    }

    private void ProcessLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (ThrowIntention)
            {
                Instantiate(Resources.Load<GameObject>("TargetMarker/TargetMarker"), hit.point, Quaternion.identity);
                var action = new ThrowAction
                {
                    Destination = ProtobufUtils.ToVector3ProtoBuf(hit.point)
                };
                Client.InputAction(action);
                ThrowIntention = false;
                return;
            }
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PlayerSelection.SelectPlayer(hit.collider.gameObject);
            }
        }
    }

    private void ProcessRightClick()
    {
        var selectedPlayer = PlayerSelection.GetSelected();
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            var action = new PlayerMovementAction
            {
                PlayerId = selectedPlayer.GetComponent<PlayerController>().ID,
                X = hit.point.x,
                Y = hit.point.z
            };
            Client.InputAction(action);
        }
    }

    private void ProcessStop()
    {
        var selectedPlayer = PlayerSelection.GetSelected();
        var action = new PlayerStopAction
        {
            PlayerId = selectedPlayer.GetComponent<PlayerController>().ID
        };
        Client.InputAction(action);
    }

    private void ProcessGrab()
    {
        var selectedPlayer = PlayerSelection.GetSelected();

        selectedPlayer.GetComponent<GrabManager>().SetGrabIntention();
    }
}
