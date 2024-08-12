using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public TextMeshProUGUI Selection;
    public int PlayerNumber;

    public Material SelectedPlayerNumber;
    public Material UnselectedPlayerNumber;

    private void Start()
    {
        if (!GetComponent<PlayerController>().IsMy)
        {
            enabled = false;
            Selection.gameObject.SetActive(false);
            return;
        }
        
        Selection.gameObject.SetActive(true);
        Selection.fontMaterial = UnselectedPlayerNumber;
        Selection.text = PlayerNumber.ToString();
    }

    public void Select()
    {
        Selection.fontMaterial = SelectedPlayerNumber;
    }

    public void Unselect()
    {
        Selection.fontMaterial = UnselectedPlayerNumber;
    }

    // public void SelectNext()
    // {
    //     Selection.enabled = true;
    //     Selection.material.color = SelectNextColor;
    // }
}
