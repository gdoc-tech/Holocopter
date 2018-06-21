﻿using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Tests;
using HoloToolkit.Unity;
using UnityEngine;

public class MessageManager : Singleton<MessageManager>
{
    private NetworkConnection serverConnection;
    private NetworkConnectionAdapter connectionAdapter;
    public long LocalUserID { get; set; }

    public delegate void MessageCallback(NetworkInMessage msg);

    private void LogDebugMsg(NetworkInMessage msg)
    {
        Debug.Log(msg.ReadString());
    }

    private Dictionary<HoloMessageID, MessageCallback> messageHandlers =
        new Dictionary<HoloMessageID, MessageCallback>();

    public Dictionary<HoloMessageID, MessageCallback> MessageHandlers
    {
        get { return messageHandlers; }
    }

    public enum HoloMessageID : byte
    {
        DebugMsg = HoloToolkit.Sharing.MessageID.UserMessageIDStart,
        Max
    }

    // Use this for initialization
    void Start()
    {
        if (SharingStage.Instance.IsConnected)
        {
            Connected();
        }
        else
        {
            SharingStage.Instance.SharingManagerConnected += Connected;
        }
    }

    private void Connected(object sender = null, EventArgs e = null)
    {
        SharingStage.Instance.SharingManagerConnected -= Connected;
        InitMessageHandlers();
    }

    private void InitMessageHandlers()
    {
        SharingStage sharingStage = SharingStage.Instance;
        if (sharingStage == null)
        {
            Debug.Log("Cannot Initialize CustomMessages. No SharingStage instance found.");
            return;
        }

        serverConnection = sharingStage.Manager.GetServerConnection();
        if (serverConnection == null)
        {
            Debug.Log("Cannot initialize CustomMessages. Cannot get a server connection.");
            return;
        }

        connectionAdapter = new NetworkConnectionAdapter();
        connectionAdapter.MessageReceivedCallback += OnMessageReceived;
        connectionAdapter.ConnectedCallback += OnConnected;

        LocalUserID = SharingStage.Instance.Manager.GetLocalUser().GetID();

        MessageHandlers.Add(HoloMessageID.DebugMsg, LogDebugMsg);
        serverConnection.AddListener((byte) HoloMessageID.DebugMsg, connectionAdapter);

        InvokeRepeating("SendDebugMessage", 1.0f, 2.0f);
    }

    private void OnConnected(NetworkConnection connection)
    {
        Debug.Log("P: Client connected!");
    }

    private void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
    {
        Debug.Log("Messesage Received...");
        byte messageType = msg.ReadByte();
        MessageCallback messageHandler = MessageHandlers[(HoloMessageID) messageType];
        if (messageType != null)
        {
            messageHandler(msg);
        }
    }

    public void SendDebugMessage()
    {
        Debug.Log("Debug message invoked");

//        string debugMsg = "debug...";
        NetworkOutMessage msg = CreateMessage((byte) HoloMessageID.DebugMsg);
        msg.Write(1f);
        serverConnection.Broadcast(msg);
    }

    private NetworkOutMessage CreateMessage(byte messageType)
    {
        NetworkOutMessage msg = serverConnection.CreateMessage(messageType);
        msg.Write(messageType);
        msg.Write(LocalUserID);
        return msg;
    }

    // Update is called once per frame
    void Update()
    {
    }
}