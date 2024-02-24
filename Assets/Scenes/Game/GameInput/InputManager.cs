using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Client Client;
    
    private GameObject selectedPlayer;

    private bool ThrowIntention;

    private void Start()
    {
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SelectNextPlayer();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ProcessThroughIntention();
        }
    }

    void ProcessThroughIntention()
    {
        ThrowIntention = true;
    }

    void DeselectPlayer()
    {
        if (selectedPlayer == null) { return; }

        selectedPlayer.transform.Find("Body").GetComponent<Outline>().OutlineWidth = 0;
        selectedPlayer = null;
    }

    void SelectPlayer(GameObject selected)
    {
        selectedPlayer = selected;
        selectedPlayer.transform.Find("Body").GetComponent<Outline>().OutlineWidth = PlayerConfig.OutlineWidth;
    }

    void SelectNextPlayer()
    {
        if (selectedPlayer == null)
        {
            return;
            
        }
        if (!selectedPlayer.GetComponent<PlayerController>().IsMy) { return; }

        List<uint> myIDs = new List<uint>();
        List<GameObject> myPlayers = new List<GameObject>();
        var client = GameObject.FindWithTag("Client").GetComponent<Client>();
        foreach (var player in client.GetPlayers().Values)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller.IsMy)
            {
                myIDs.Add(controller.ID);
                myPlayers.Add(player.gameObject);
            }
        }

        
        var curID = selectedPlayer.GetComponent<PlayerController>().ID;
        var curInd = myIDs.FindIndex((uint x) => x == curID);
        var nextInd = (curInd + 1) % myIDs.Count;
        
        DeselectPlayer();
        SelectPlayer(myPlayers[nextInd]);
    }

    private void ProcessLeftClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = (1 << LayerMask.NameToLayer("Server")) +
                        (1 << LayerMask.NameToLayer("Dummy")) + 
                        (1 << LayerMask.NameToLayer("ClientServer")) + 
                        (1 << LayerMask.NameToLayer("DummyServer"));
        layerMask = ~layerMask;

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
                DeselectPlayer();
                SelectPlayer(hit.collider.gameObject);
            }
            if (hit.collider.gameObject.CompareTag("Floor"))
            {
                DeselectPlayer();
            }
        }
    }

    private void ProcessRightClick()
    {
        if (selectedPlayer == null)
        {
            return;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = (1 << LayerMask.NameToLayer("Server")) +
                        (1 << LayerMask.NameToLayer("Dummy")) + 
                        (1 << LayerMask.NameToLayer("ClientServer")) + 
                        (1 << LayerMask.NameToLayer("DummyServer"));
        layerMask = ~layerMask;

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
        if (selectedPlayer == null)
        {
            return;
            
        }
        var action = new PlayerStopAction
        {
            PlayerId = selectedPlayer.GetComponent<PlayerController>().ID
        };
        Client.InputAction(action);
    }

    private void ProcessGrab()
    {
        if (selectedPlayer == null)
        {
            return;
        }

        var action = new GrabAction
        {
            PlayerId = selectedPlayer.GetComponent<PlayerController>().ID
        };
        Client.InputAction(action);
    }
}
