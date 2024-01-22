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
}
