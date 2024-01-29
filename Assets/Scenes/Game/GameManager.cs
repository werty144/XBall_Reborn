// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Google.Protobuf;
// using Steamworks;
// using UnityEngine;
// using UnityEngine.Assertions;
// using UnityEngine.PlayerLoop;
//
// public struct FieldParams
// {
//     public float Width;
//     public float Length;
// }
//
// public class GameManager : MonoBehaviour
// {
//     public GameObject PlayerPrefab;
//     
//     private P2P P2PManager;
//     private GameStarter GameStarter;
//     private UIManagerGame _uiManagerGame;
//     
//     private Dictionary<uint, PlayerController> Players = new ();
//     private CSteamID OpponentID;
//     private bool IAmMaster;
//     private FieldParams FieldParams;
//
//     private GameStateVersioning GameStateVersioning;
//     private TimeSpan Ping;
//
//     private bool GameStarted;
//
//     // ----------------------------- SETUP --------------------------------
//     public void Setup(SetupInfo setupInfo)
//     {
//         Debug.Log("Setting up game manager");
//         OpponentID = setupInfo.OpponentID;
//         IAmMaster = setupInfo.IAmMaster;
//         
//         SetFieldParams();
//         CreatePlayers(setupInfo.NumberOfPlayers);
//
//         GameStateVersioning = new GameStateVersioning(this);
//         
//         GameStarter.GameManagerReady(this);
//     }
//     
//     void OnEnable()
//     {
//         var global = GameObject.FindWithTag("Global");
//         P2PManager = global.GetComponent<P2P>();
//         GameStarter = global.GetComponent<GameStarter>();
//         _uiManagerGame = GameObject.FindWithTag("UIManager").GetComponent<UIManagerGame>();
//         
//         var setupInfo = global.GetComponent<GameStarter>().GetSetupInfo();
//         Setup(setupInfo);
//     }
//     
//     void CreatePlayers(int n)
//     {
//         byte spareID = 0;
//         float masterZ = -FieldParams.Length / 4;
//         float followerZ = FieldParams.Length / 4;
//         for (int i = 0; i < n; i++)
//         {
//             var x = FieldParams.Width * (i + 1) / (n + 1) - FieldParams.Width / 2;
//             
//             var masterPlayer = Instantiate(PlayerPrefab, 
//                 new Vector3(x, PlayerConfig.Height, masterZ), Quaternion.identity);
//             var controller = masterPlayer.GetComponent<PlayerController>();
//             controller.Initialize(IAmMaster, spareID);
//             Players[spareID] = controller;
//             spareID++;
//             
//             var followerPlayer = Instantiate(PlayerPrefab, 
//                 new Vector3(x, PlayerConfig.Height, followerZ), Quaternion.identity);
//             var followerContorller = followerPlayer.GetComponent<PlayerController>();
//             followerContorller.Initialize(!IAmMaster, spareID);
//             Players[spareID] = followerContorller;
//             spareID++;
//         }
//     }
//
//     void SetFieldParams()
//     {
//         var defaultPlaneWidth = 10;
//         var defaultPlaneLength = 10;
//         
//         var floor = GameObject.FindWithTag("Floor");
//         var scale = floor.GetComponent<Transform>().localScale;
//         FieldParams.Width = scale.x * defaultPlaneWidth;
//         FieldParams.Length = scale.z * defaultPlaneLength;
//     }
//
//     public void StartGame()
//     {
//         GameStarted = true;
//     }
//     
//     // -------------------------------------- SERVER-CLIENT ------------------------------
//     void LateUpdate()
//     {
//         if (!GameStarted) {return;}
//         
//         GameStateVersioning.AddCurrentState(GetGameState());
//     }
//
//     public void InputAction(IBufferMessage action)
//     {
//         GameStateVersioning.AddCurrentAction(action);
//         // No matter if master or follower, 
//         // Do optimistic execution
//         switch (action)
//         {
//             case PlayerMovementAction playerMovementAction:
//                 var player = Players[playerMovementAction.Id];
//                 //Invalid action
//                 if (!player.IsMy) { return; }
//
//                 var target = new Vector2(playerMovementAction.X, playerMovementAction.Y);
//                 player.SetTarget(target);
//                 break;
//             default:
//                 Debug.LogWarning("Unknown input action");
//                 break;
//         }
//
//         if (IAmMaster)
//         {
//             P2PManager.SendGameStateMessage(OpponentID, GetGameState());
//         }
//         else
//         {
//             P2PManager.SendAction(OpponentID, action);
//         }
//     }
//
//     public void OpponentAction(IBufferMessage action)
//     {
//         Assert.IsTrue(IAmMaster, "Process opponent's action only as a server");
//         
//         GameStateVersioning.ApplyActionInThePast(action, Ping);
//         P2PManager.SendGameStateMessage(OpponentID, GetGameState());
//     }
//
//     public void ReceiveGameState(GameState newGameState)
//     {
//         Assert.IsFalse(IAmMaster, "Only receive game states as a follower");
//         var initialState = GetGameState();
//         ApplyGameState(newGameState);
//         GameStateVersioning.FastForward(Ping);
//         GameStateVersioning.SmoothFromPast(initialState);
//     }
//
//     public void GameEnd()
//     {
//         P2PManager.CloseConnection();
//     }
//     
//     // ------------------------ GETTERS ------------------------- //
//     
//     public GameState GetGameState()
//     {
//         GameState gameState = new GameState();
//         foreach (var player in Players.Values)
//         {
//             gameState.PlayerStates.Add(player.GetState());
//         }
//
//         return gameState;
//     }
//
//     public Dictionary<uint, PlayerController> GetPlayers()
//     {
//         return Players;
//     }
//     
//     public bool IsMaster()
//     {
//         return IAmMaster;
//     }
//     
//     // ----------------------------- SETTERS ----------------------------
     // public void ApplyGameState(GameState state)
     // {
     //     foreach (var playerState in state.PlayerStates)
     //     {
     //         Players[playerState.Id].ApplyState(playerState);
     //     }
     // }
//     
//     public void LastRTT(TimeSpan rtt)
//     {
//         Ping = rtt / 2;
//         _uiManagerGame.UpdatePing(Ping);
//     }
// }
