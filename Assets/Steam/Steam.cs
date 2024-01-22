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

    public static void SendInvite(CSteamID id)
    {
        Debug.Log("Sent invite to " + id);
    }

    public static Dictionary<string, string> GetLobbyMetaData(ulong lobbyID)
    {
        var dict = new Dictionary<string, string>
        {
            { "SpeedKey", "slow" }
        };
        return dict;
    }

    public static SteamUser[] GetLobbyMembers(ulong lobbyID)
    {
        SteamUser[] lobbyMembers =
        {
            new SteamUser(new CSteamID(1), "Anton"),
            new SteamUser(new CSteamID(2), "Grisha")
        };
        return lobbyMembers;
    }
    
    public static string GetLobbyMemberData(ulong lobbyID, CSteamID userID, string key) {
        // SteamMatchmaking.GetLobbyMemberData()
        return "true";
    }

    public static void SetLobbyMyData(ulong lobbyID, string key, string value)
    {
        
    }
}
