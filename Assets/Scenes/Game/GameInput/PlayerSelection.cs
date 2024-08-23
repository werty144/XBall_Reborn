using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    private Dictionary<int, SelectionManager> MyPlayers = new();
    private SelectionManager CurrentSelected;
    void Start()
    {
        var client = GameObject.FindWithTag("Client").GetComponent<Client>();
        foreach (var player in client.GetPlayers().Values.Where(player => player.IsMy))
        {
            var selectionManager = player.gameObject.GetComponent<SelectionManager>();
            MyPlayers[selectionManager.PlayerNumber] = selectionManager;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int number = 0 ;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            number = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            number = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            number = 3;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            number = 4;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            number = 5;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            number = 6;
        }

        if (number == 0) return;
        if (!MyPlayers.Keys.Contains(number)) return;
        
        if (CurrentSelected != null) CurrentSelected.Unselect();
        CurrentSelected = MyPlayers[number];
        CurrentSelected.Select();
    }

    public void SelectPlayer(GameObject player)
    {
        if (!player.GetComponent<PlayerController>().IsMy) {return;}
        
        if (CurrentSelected != null) CurrentSelected.Unselect();
        CurrentSelected = player.GetComponent<SelectionManager>();
        CurrentSelected.Select();
    }

    public GameObject GetSelected()
    {
        return CurrentSelected == null ? null : CurrentSelected.gameObject;
    }
}
