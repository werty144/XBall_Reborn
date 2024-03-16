using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private MeshRenderer Selection;

    private Color SelectColor = new Color(82f/255, 186f/255, 221f/255, 1);
    private Color SelectNextColor = new Color(111f/255, 111f/255, 111f/255, 0.5f);
    void Start()
    {
        Selection = transform.Find("Selection").gameObject.GetComponent<MeshRenderer>();
        Selection.enabled = false;
    }

    public void Select()
    {
        Selection.enabled = true;
        Selection.material.color = SelectColor;
    }

    public void Unselect()
    {
        Selection.enabled = false;
    }

    public void SelectNext()
    {
        Selection.enabled = true;
        Selection.material.color = SelectNextColor;
    }
}
