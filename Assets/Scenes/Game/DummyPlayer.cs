using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class DummyPlayer : MonoBehaviour
{
    public GameObject GameManager;
    public GameObject slider;
    
    private List<PlayerController> playersToControll;
    private uint CurGameState;
    private int LagFrames;
    private int ActionFrequency = 400;
    void Start()
    {
        playersToControll = new List<PlayerController>();
        CurGameState = 0;
        LagFrames = 0;
        
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var controller = player.GetComponent<PlayerController>();
            if (!controller.IsMy)
            {
                playersToControll.Add(controller);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (CurGameState % 200 == 0)
        // {
        //     var random = new Random();
        //     var playerIndex = random.Next(0, playersToControll.Count);
        //     var playerID = playersToControll[playerIndex].ID;  
        //     var randX = random.Next(0, 20) - 10;
        //     var randY = random.Next(0, 30) - 15;
        //     var target = new Vector2(randX, randY);
        //     GameManager.GetComponent<GameManager>().OpponentAction_SetPlayerTarget(CurGameState - 100, playerID, target);
        // }
        if (CurGameState % ActionFrequency == 0 && CurGameState % (2 * ActionFrequency) != 0)
        {
            var playerID = playersToControll[0].ID;  
            var randX = 0;
            var randY = - 15;
            var target = new Vector2(randX, randY);
            GameManager.GetComponent<GameManager>().OpponentAction_SetPlayerTarget(CurGameState - (uint)LagFrames, playerID, target);
        }
        if (CurGameState % (2 * ActionFrequency) == 0)
        {
            var playerID = playersToControll[0].ID;  
            var randX = 0;
            var randY = 15;
            var target = new Vector2(randX, randY);
            GameManager.GetComponent<GameManager>().OpponentAction_SetPlayerTarget(CurGameState - (uint)LagFrames, playerID, target);
        }

        CurGameState++;
    }

    public void OnSliderChange()
    {
        LagFrames = (int) (slider.GetComponent<Slider>().value * 100f);
    }
}
