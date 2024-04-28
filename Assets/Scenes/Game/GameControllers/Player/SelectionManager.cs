using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public TextMeshProUGUI Selection;
    public int PlayerNumber;

    private Color SelectColor = new Color(82f/255, 186f/255, 221f/255, 1);
    private Color UnselectColor = new Color(111f/255, 111f/255, 111f/255, 0.5f);

    private void Start()
    {
        if (!GetComponent<PlayerController>().IsMy)
        {
            enabled = false;
            Selection.gameObject.SetActive(false);
            return;
        }
        
        Selection.gameObject.SetActive(true);
        Selection.color = UnselectColor;
        Selection.text = PlayerNumber.ToString();
    }

    public void Select()
    {
        Selection.color = SelectColor;
    }

    public void Unselect()
    {
        Selection.color = UnselectColor;
    }

    // public void SelectNext()
    // {
    //     Selection.enabled = true;
    //     Selection.material.color = SelectNextColor;
    // }
}
