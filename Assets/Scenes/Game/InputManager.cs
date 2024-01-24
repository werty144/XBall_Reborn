using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject GameManager;
    
    private GameObject selectedPlayer;

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
                GameManager.GetComponent<GameManager>().InputAction_SetPlayerTarget(selectedPlayer, vectorPlane);
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
        selectedPlayer.GetComponent<Outline>().OutlineWidth = PlayerParams.OutlineWidth;
    }
}
