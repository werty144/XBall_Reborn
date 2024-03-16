using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    private List<SelectionManager> MyPlayers = new();
    private SelectionManager CurrentSelected;
    void Start()
    {
        var client = GameObject.FindWithTag("Client").GetComponent<Client>();
        foreach (var player in client.GetPlayers().Values.Where(player => player.IsMy))
        {
            MyPlayers.Add(player.gameObject.GetComponent<SelectionManager>());
        }
        MyPlayers[0].Select();
        CurrentSelected = MyPlayers[0];
        GetNextToSelect().SelectNext();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CurrentSelected.Unselect();
            var nextToSelect = GetNextToSelect();
            nextToSelect.Unselect();
            
            CurrentSelected = nextToSelect;
            CurrentSelected.Select();
            nextToSelect = GetNextToSelect();
            nextToSelect.SelectNext();
        }
    }

    public void SelectPlayer(GameObject player)
    {
        if (!player.GetComponent<PlayerController>().IsMy) {return;}
        
        CurrentSelected.Unselect();
        var nextToSelect = GetNextToSelect();
        nextToSelect.Unselect();

        CurrentSelected = player.GetComponent<SelectionManager>();
        CurrentSelected.Select();
        GetNextToSelect().SelectNext();
    }

    private SelectionManager GetNextToSelect()
    {
        var currentIndex = MyPlayers.FindIndex(manager => manager.gameObject.GetInstanceID() == CurrentSelected.gameObject.GetInstanceID());
        var nextIndex = (currentIndex + 1) % MyPlayers.Count;
        return MyPlayers[nextIndex];
    }

    public GameObject GetSelected()
    {
        return CurrentSelected.gameObject;
    }
}
