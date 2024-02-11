using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class PingManager : MonoBehaviour
{
    private MessageManager MessageManager;
    
    private DateTime LastPingSent;
    private TimeSpan Ping;

    private void Start()
    {
        MessageManager = GameObject.FindWithTag("P2P").GetComponent<MessageManager>();
    }

    public void OnConnected()
    {
        SendPing();
    }

    private void SendPing()
    {
        LastPingSent = DateTime.Now;
        MessageManager.SendPing();
    }

    public void ReceivePing()
    {
        MessageManager.SendReplyPing();
    }

    public void ReceiveReplyPing()
    {
        Ping = (DateTime.Now - LastPingSent) / 2;
        SendPing();
    }

    public virtual TimeSpan GetPingToUser(CSteamID userID)
    {
        return userID == Steam.MySteamID() ? TimeSpan.Zero : Ping;
    }

    public TimeSpan GetPing()
    {
        return Ping;
    }
}
