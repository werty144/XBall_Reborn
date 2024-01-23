using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class LobbyManager : MonoBehaviour
{
    private const string ReadyStatusKey = "ReadyStatusKey";
    private const string SpeedKey = "SpeedKey";
    
    private ulong currentLobbyID;
    private ulong pendingInvitedID;

    public GameObject lobbyMemberPrefab;
    public Transform membersContainer;
    public Transform metaData;

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
    }

    public void OnLeave()
    {
        currentLobbyID = 0;
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
        var exampleMetaData = metaData.transform.Find("ExampleName").GetComponent<TextMeshProUGUI>();
        if (exampleMetaData == null)
        {
            Debug.LogWarning("Example meta data is not found");
        }
        else
        {
            var lobbyMetaData = Steam.GetLobbyMetaData(currentLobbyID);
            exampleMetaData.text = lobbyMetaData[SpeedKey];
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
}
