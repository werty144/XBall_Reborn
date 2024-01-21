using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        print("Jopa");
        GetFriends();
    }

    void GetFriends()
    {
        {
            if (!SteamManager.Initialized) { return; }

            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            List<CSteamID> onlineFriends = new List<CSteamID>();

            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                EPersonaState state = SteamFriends.GetFriendPersonaState(friendSteamId);

                if (state == EPersonaState.k_EPersonaStateOnline)
                {
                    onlineFriends.Add(friendSteamId);
                    string friendName = SteamFriends.GetFriendPersonaName(friendSteamId);
                    print("Online Friend: " + friendName);
                }
            }

            // Now onlineFriends contains the list of online friends
        }
    }
}
