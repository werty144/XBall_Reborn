using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Client Client;
    
    private GameObject selectedPlayer;

    private void Start()
    {
        Client = GameObject.FindWithTag("Client").GetComponent<Client>();
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
                var action = new PlayerMovementAction
                {
                    PlayerId = selectedPlayer.GetComponent<PlayerController>().ID,
                    X = hit.point.x,
                    Y = hit.point.z
                };
                Client.InputAction(action);
            }
        }

        if (Input.GetKeyDown(KeyCode.S) && selectedPlayer != null)
        {
            var action = new PlayerStopAction
            {
                PlayerId = selectedPlayer.GetComponent<PlayerController>().ID
            };
            Client.InputAction(action);
        }

        if (Input.GetKeyDown(KeyCode.Tab) && selectedPlayer != null)
        {
            SelectNextPlayer();
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

    void SelectNextPlayer()
    {
        if (!selectedPlayer.GetComponent<PlayerController>().IsMy) { return; }

        List<uint> myIDs = new List<uint>();
        List<GameObject> myPlayers = new List<GameObject>();
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller.IsMy)
            {
                myIDs.Add(controller.ID);
                myPlayers.Add(player);
            }
        }

        
        var curID = selectedPlayer.GetComponent<PlayerController>().ID;
        var curInd = myIDs.FindIndex((uint x) => x == curID);
        var nextInd = (curInd + 1) % myIDs.Count;
        
        DeselectPlayer();
        SelectPlayer(myPlayers[nextInd]);
    }
}
