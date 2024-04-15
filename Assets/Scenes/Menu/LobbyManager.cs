using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LobbyManager : MonoBehaviour
{
    private const string ReadyStatusKey = "ReadyStatusKey";
    private const string NumberOfPlayersKey = "NumberOfPlayersKey";
    private const string SpeedKey = "SpeedKey";

    public const string SpeedSlow = "SpeedSlow";
    public const string SpeedNormal = "SpeedNormal";
    public const string SpeedFast = "SpeedFast";
    
    private ulong currentLobbyID;
    private ulong pendingInvitedID;

    public GameObject lobbyMemberPrefab;
    public Transform membersContainer;
    public Transform numberOfPlayersDropdown;
    public Transform speedDropdown;

    public void InviteAndCreateOnNeed(CSteamID invitedID)
    {
        // if (currentLobbyID == 0)
        // {
            pendingInvitedID = invitedID.m_SteamID;
            Steam.CreateLobby();
        // }
        // else
        // {
        //     print("Sending invite");
        //     Steam.SendInvite(currentLobbyID, invitedID.m_SteamID);
        // }
    }

    public void OnLobbyCreate(ulong lobbyID)
    {
        Steam.SetLobbyMetaData(lobbyID, NumberOfPlayersKey, 2.ToString());
        Steam.SetLobbyMetaData(lobbyID, SpeedKey, SpeedNormal);
    }

    public void OnLobbyEnter(ulong lobbyID)
    {
        currentLobbyID = lobbyID;
        SetReady(false);

        if (pendingInvitedID != 0)
        {
            print("Iiinvite");
            Steam.SendInvite(currentLobbyID, pendingInvitedID);
            pendingInvitedID = 0;
        }
    }

    public void OnDataUpdate()
    {
        UpdateData();
        OwnerOrNot();
        CheckAllReady();
    }

    public void OnLeave()
    {
        Steam.LeaveLobby(currentLobbyID);
        currentLobbyID = 0;
        ClearMembers();
    }

    private void UpdateData()
    {
        if (currentLobbyID == 0)
        {
            Debug.LogWarning("Update lobby data while not in lobby");
            return;
        }
        UpdateMembers();
        UpdateMetaData();
    }

    private void UpdateMetaData()
    {
        var playersDropdown = numberOfPlayersDropdown.GetComponent<TMP_Dropdown>();
        if (playersDropdown == null)
        {
            Debug.LogWarning("Failed to get number of players dropdown");
            return;
        }
        var numberOfPlayers = GetNumberOfPlayers();
        if (numberOfPlayers == -1)
        {
            return;
        }
        playersDropdown.value = numberOfPlayers - 1;
        

        var speedDropdown_ = this.speedDropdown.GetComponent<TMP_Dropdown>();
        if (speedDropdown_ == null)
        {
            Debug.LogWarning("Failed to get speed dropdown");
            return;
        }
        var speed = GetSpeed();
        switch (speed)
        {
            case SpeedSlow:
                speedDropdown_.value = 0;
                break;
            case SpeedNormal:
                speedDropdown_.value = 1;
                break;
            case SpeedFast:
                speedDropdown_.value = 2;
                break;
            default:
                Debug.LogWarning("Unknown speed");
                return;
        }
    }

    private void UpdateMembers()
    {
        ClearMembers();
        var lobbyMembers = Steam.GetLobbyMembers(currentLobbyID);

        foreach (var member in lobbyMembers)
        {
            GameObject memberPanel = Instantiate(lobbyMemberPrefab, membersContainer);
            
            var nicknamePanel = memberPanel.transform.Find("Nickname").GetComponent<TextMeshProUGUI>();
            if (nicknamePanel == null)
            {
                Debug.LogWarning("Nickname panel of lobby member prefab not found");
            }
            else
            {
                nicknamePanel.text = member.Name;
            }
            
            var readyPanel = memberPanel.transform.Find("ReadyStatus").GetComponent<TextMeshProUGUI>();
            if (readyPanel == null)
            {
                Debug.LogWarning("Ready Status of lobby member prefab not found");
            }
            else
            {
                readyPanel.text = Steam.GetLobbyMemberData(currentLobbyID, member.ID, ReadyStatusKey);
            }
            
            var readyButton = memberPanel.transform.Find("ReadyButton").GetComponent<Button>();
            if (readyButton == null)
            {
                Debug.LogWarning("Ready button of lobby member prefab not found");
            }
            else
            {
                if (member.ID == Steam.MySteamID())
                {
                    readyButton.onClick.AddListener(OnReadyChange);
                }
                else
                {
                    memberPanel.transform.Find("ReadyButton").SetParent(null);
                }
            }
        }
    }

    private void ClearMembers()
    {
        for (int i = membersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(membersContainer.GetChild(i).gameObject);
        }
    }

    private void SetReady(bool ready)
    {
        if (currentLobbyID == 0)
        {
            Debug.LogWarning("Set ready while not in lobby");
            return;
        }
        Steam.SetLobbyMyData(currentLobbyID, ReadyStatusKey, ready.ToString());
    }

    public void OnReadyChange()
    {
        if (currentLobbyID == 0)
        {
            Debug.LogWarning("On ready change while not in lobby");
            return;
        }
        
        var readyStatus = Steam.GetLobbyMemberData(currentLobbyID, Steam.MySteamID(), ReadyStatusKey);
        bool boolValue;
        var valid = bool.TryParse(readyStatus, out boolValue);
        if (!valid)
        {
            Debug.LogWarning("Ready status is not valid");
            return;
        }
        SetReady(!boolValue);
    }

    public void OnNumberOfPlayersChange()
    {
        if (currentLobbyID == 0)
        {
            Debug.LogWarning("On number of players change while not in lobby");
            return;
        }

        if (Steam.GetLobbyOwner(currentLobbyID) != Steam.MySteamID())
        {
            // This means that the owner have changed the players number.
            // We don't need to do anything
            return;
        }
        
        var dropdown = numberOfPlayersDropdown.GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogWarning("Failed to get number of players dropdown");
            return;
        }
        var newNumberOfPlayers = dropdown.value + 1;
        Steam.SetLobbyMetaData(currentLobbyID, NumberOfPlayersKey, newNumberOfPlayers.ToString());
    }

    public void OnSpeedChange()
    {
        if (currentLobbyID == 0)
        {
            Debug.LogWarning("On speed change while not in lobby");
            return;
        }
        if (Steam.GetLobbyOwner(currentLobbyID) != Steam.MySteamID())
        {
            // This means that the owner have changed the speed.
            // We don't need to do anything
            return;
        }
        var dropdown = speedDropdown.GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogWarning("Failed to get speed dropdown");
            return;
        }

        string speed;
        switch (dropdown.value)
        {
            case 0:
                speed = SpeedSlow;
                break;
            case 1:
                speed = SpeedNormal;
                break;
            case 2:
                speed = SpeedFast;
                break;
            default:
                Debug.LogWarning("Unknown speed dropdown value");
                return;
        }
        Steam.SetLobbyMetaData(currentLobbyID, SpeedKey, speed);
    }

    private void CheckAllReady()
    {
        var members = Steam.GetLobbyMembers(currentLobbyID);

        if (members.Count < 2) { return; }

        bool allReady = true;
        foreach (var member in members)
        {
            var readyStatusString = Steam.GetLobbyMemberData(currentLobbyID, member.ID, ReadyStatusKey);
            bool readyStatus;
            var success = bool.TryParse(readyStatusString, out readyStatus);
            if (!success)
            {
                Debug.LogWarning("Invalid ready status");
            }

            allReady = allReady && readyStatus;
        }

        if (allReady)
        {
            SwitchScenes();
        }
    }

    private void SwitchScenes()
    {
        Assert.AreNotEqual(0, currentLobbyID, "Switch scenes not in lobby");
        var iAmOwner = Steam.GetLobbyOwner(currentLobbyID) == Steam.MySteamID();
        var numberOfPlayers = GetNumberOfPlayers();
        Assert.AreNotEqual(-1, numberOfPlayers, "Can't get number of players");
        var opponentID = GetPartnerID();
        Assert.AreNotEqual(CSteamID.Nil, opponentID, "Can't get opponent ID");
        var speed = GetSpeed();
        
        GameObject.FindWithTag("Global").GetComponent<GameStarter>().Initiate(
            new SetupInfo
            {
                IAmMaster =  iAmOwner,
                NumberOfPlayers = numberOfPlayers,
                Speed = speed,
                OpponentID = opponentID,
                MyID = Steam.MySteamID()
            });
        Steam.LeaveLobby(currentLobbyID);
    }

    private void OwnerOrNot()
    {
        var playersDropdown = numberOfPlayersDropdown.GetComponent<TMP_Dropdown>();
        if (playersDropdown == null)
        {
            Debug.LogWarning("Failed to get number of players dropdown");
            return;
        }

        var speedDropdown_ = speedDropdown.GetComponent<TMP_Dropdown>();
        
        if (Steam.GetLobbyOwner(currentLobbyID) == Steam.MySteamID())
        {
            playersDropdown.interactable = true;
            speedDropdown_.interactable = true;
        }
        else
        {
            playersDropdown.interactable = false;
            speedDropdown_.interactable = false;
        }
    }

    private CSteamID GetPartnerID()
    {
        if (currentLobbyID == 0)
        {
            Debug.LogWarning("Get partner's ID while not in lobby");
            return CSteamID.Nil;
        }

        var members = Steam.GetLobbyMembers(currentLobbyID);
        if (members.Count < 2)
        {
            return CSteamID.Nil;
        }

        if (members[0].ID != Steam.MySteamID())
        {
            return members[0].ID;
        }
        else
        {
            return members[1].ID;
        }
    }

    private int GetNumberOfPlayers()
    {
        var numberOfPlayersString = Steam.GetLobbyMetaData(currentLobbyID, NumberOfPlayersKey);
        Int32 numberOfPlayers;
        var valid = Int32.TryParse(numberOfPlayersString, out numberOfPlayers);
        if (!valid || numberOfPlayers < 1)
        {
            Debug.LogWarning("Invalid number of players in meta data: " + numberOfPlayersString);
            return -1;
        }

        return numberOfPlayers;
    }

    private string GetSpeed()
    {
        var speed = Steam.GetLobbyMetaData(currentLobbyID, SpeedKey);
        Assert.IsTrue(speed == SpeedSlow || speed == SpeedNormal || speed == SpeedFast);
        return speed;
    }

    public void TestCreateLobby()
    {
        Steam.CreateLobby();
    }
}
