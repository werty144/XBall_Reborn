using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public struct SteamUser
{
    public CSteamID ID;
    public string Name;

    public SteamUser(CSteamID id, string nm)
    {
        ID = id;
        Name = nm;
    }
}

public class Steam
{

    public static CSteamID MySteamID()
    {
        if (!SteamManager.Initialized) { return new CSteamID(0); }
        
        return Steamworks.SteamUser.GetSteamID();
    }
    public static List<SteamUser> GetOnlineFriends()
    {
        if (!SteamManager.Initialized) { return new List<SteamUser>(); }

        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        List<SteamUser> onlineFriends = new List<SteamUser>();

        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            EPersonaState state = SteamFriends.GetFriendPersonaState(friendSteamId);

            if (state == EPersonaState.k_EPersonaStateOnline)
            {
                string friendName = SteamFriends.GetFriendPersonaName(friendSteamId);
                onlineFriends.Add(new SteamUser(friendSteamId, friendName));
            }
        }

        return onlineFriends;
    }

    public static void SendInvite(ulong lobbyID, ulong invitedID)
    {
        var success = SteamMatchmaking.InviteUserToLobby(new CSteamID(lobbyID), new CSteamID(invitedID));
        if (success)
        {
            Debug.Log("Sent invite for lobby " + lobbyID + " to " + invitedID);
        }
        else
        {
            Debug.LogWarning("Failed to send a lobby invite");
        }
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 2);
    }

    public static string GetLobbyMetaData(ulong lobbyID, string key)
    {
        var result = SteamMatchmaking.GetLobbyData(new CSteamID(lobbyID), key);
        if (result == "")
        {
            Debug.LogWarning("No meta data for " + key);
        }

        return result;
    }

    public static void SetLobbyMetaData(ulong lobbyID, string key, string value)
    {
        var success = SteamMatchmaking.SetLobbyData(new CSteamID(lobbyID), key, value);
        if (!success)
        {
            Debug.LogWarning("Failed to set lobby meta data - " + key + ":" + value);
        }
    }

    public static List<SteamUser> GetLobbyMembers(ulong lobbyID)
    {
        List<SteamUser> lobbyMembers = new List<SteamUser>();
        var lobbyId = new CSteamID(lobbyID);
        int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

        for (int i = 0; i < memberCount; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            var memberName = SteamFriends.GetFriendPersonaName(memberId);
            lobbyMembers.Add(new SteamUser(memberId, memberName));
        }
        return lobbyMembers;
    }
    
    public static string GetLobbyMemberData(ulong lobbyID, CSteamID userID, string key)
    {
        return SteamMatchmaking.GetLobbyMemberData(new CSteamID(lobbyID), userID, key);
    }

    public static void SetLobbyMyData(ulong lobbyID, string key, string value)
    {
        SteamMatchmaking.SetLobbyMemberData(new CSteamID(lobbyID), key, value);
    }

    public static CSteamID GetLobbyOwner(ulong lobbyID)
    {
        return SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
    }

    public static void LeaveLobby(ulong lobbyID)
    {
        if (!SteamManager.Initialized) { return; }
        SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
    }
}
