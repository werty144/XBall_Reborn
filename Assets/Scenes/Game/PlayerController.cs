using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;


public struct PlayerParams
{
    // The height of the center of the cylinder
    public static float Height = 1;
    public static float Radius = 0.5F;

    public static float OutlineWidth = 4;

    public static Color MyColor = new Color(100/255f, 149/255f, 237/255f);
    public static Color OpponentColor = new Color(205f/255f, 92f/255f, 92f/255f, 1f);
    public static Color OutlineColor = new Color(100f/255f, 239f/255f, 213f/255f, 1f);

}

public class PlayerController : MonoBehaviour
{
    public bool IsMy;
    public uint ID;
    
    private float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    public void Initialize(bool isMy, uint id)
    {
        IsMy = isMy;
        ID = id;
    }
    void Start()
    {
        GetComponent<Outline>().OutlineWidth = 0;
        GetComponent<Outline>().OutlineColor = PlayerParams.OutlineColor;
        GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineAll;

        if (IsMy)
        {
            GetComponent<Renderer>().material.color = PlayerParams.MyColor;
        }
        else
        {
            GetComponent<Renderer>().material.color = PlayerParams.OpponentColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
    }

    public void SetTarget(Vector2 target)
    {
        var target3D = new Vector3(target.x, PlayerParams.Height, target.y);
        targetPosition = target3D;
        isMoving = true;
    }
    
    void MovePlayer()
    {
        if (!isMoving) { return; }
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
        }
    }
}
