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
    
    private ulong currentLobbyID;
    private ulong pendingInvitedID;

    public GameObject lobbyMemberPrefab;
    public Transform membersContainer;
    public Transform numberOfPlayersDropdown;
    public GameObject global;

    private void OnEnable()
    {
        global = GameObject.FindWithTag("Global");
        global.GetComponent<Callbacks>().SetLobby(this);
    }

    public void Start()
    {
        pendingInvitedID = 0;
        currentLobbyID = 0;
    }

    public void InviteAndCreateOnNeed(CSteamID invitedID)
    {
        if (currentLobbyID == 0)
        {
            pendingInvitedID = invitedID.m_SteamID;
            Steam.CreateLobby();
        }
        else
        {
            Steam.SendInvite(currentLobbyID, invitedID.m_SteamID);
        }
    }

    public void OnLobbyCreate(ulong lobbyID)
    {
        Steam.SetLobbyMetaData(lobbyID, NumberOfPlayersKey, 2.ToString());
    }

    public void OnLobbyEnter(ulong lobbyID)
    {
        currentLobbyID = lobbyID;
        SetReady(false);

        if (pendingInvitedID != 0)
        {
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
        var dropdown = numberOfPlayersDropdown.GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogWarning("Failed to get number of players dropdown");
            return;
        }
        else
        {
            var numberOfPlayers = GetNumberOfPlayers();
            if (numberOfPlayers == -1)
            {
                return;
            }
            dropdown.value = numberOfPlayers - 1;
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
        
        global.GetComponent<GameStarter>().Initiate(
            new SetupInfo
            {
                IAmMaster =  iAmOwner,
                NumberOfPlayers = numberOfPlayers,
                OpponentID = opponentID
            },
            currentLobbyID);
    }

    private void OwnerOrNot()
    {
        var dropdown = numberOfPlayersDropdown.GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogWarning("Failed to get number of players dropdown");
            return;
        }
        
        if (Steam.GetLobbyOwner(currentLobbyID) == Steam.MySteamID())
        {
            dropdown.interactable = true;
        }
        else
        {
            dropdown.interactable = false;
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

    public void TestCreateLobby()
    {
        Steam.CreateLobby();
    }
}
