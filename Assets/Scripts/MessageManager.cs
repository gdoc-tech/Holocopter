﻿using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Tests;
using HoloToolkit.Unity;
using UnityEngine;


// SendMessage is 400 times slower than directly call.
public class MessageManager : Singleton<MessageManager>
{
    public enum HoloMessageType : byte
    {
        DebugMsg = HoloToolkit.Sharing.MessageID.UserMessageIDStart,
        ChangeSlider,
        Max
    }


    private NetworkConnection _serverConnection;
    private NetworkConnectionAdapter _connectionAdapter;
    public long LocalUserId { get; set; }
    private SlidersCommands _sliderCommand;

    public delegate void MessageCallback(long userId, string msg);

    private Dictionary<HoloMessageType, MessageCallback> _messageHandlers =
        new Dictionary<HoloMessageType, MessageCallback>();


    // Use this for initialization
    void Start()
    {
        _sliderCommand = GetComponentInParent<SlidersCommands>();
        _messageHandlers = new Dictionary<HoloMessageType, MessageCallback>()
        {
            {HoloMessageType.DebugMsg, _sliderCommand.ShowServerMsg},
            {HoloMessageType.ChangeSlider, _sliderCommand.ShowServerMsg}
        };
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

        _serverConnection = sharingStage.Manager.GetServerConnection();
        if (_serverConnection == null)
        {
            Debug.Log("Cannot initialize CustomMessages. Cannot get a server connection.");
            return;
        }

        _connectionAdapter = new NetworkConnectionAdapter();
        _connectionAdapter.MessageReceivedCallback += OnMessageReceived;

        LocalUserId = SharingStage.Instance.Manager.GetLocalUser().GetID();


        foreach (var entry in _messageHandlers)
        {
            _serverConnection.AddListener((byte) entry.Key, _connectionAdapter);
        }

        InvokeRepeating("SendDebugMessage", 1.0f, 5.0f);
    }

    private NetworkOutMessage CreateMessage(byte messageType)
    {
        NetworkOutMessage msg = _serverConnection.CreateMessage(messageType);
        msg.Write(messageType);
        msg.Write(LocalUserId);
        return msg;
    }

    private void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
    {
        Debug.Log("Messesage Received...");
        var messageType = msg.ReadByte();
        var userId = msg.ReadInt64();
        string messageContent = msg.ReadString();

        var functionToCall = _messageHandlers[(HoloMessageType) messageType];
        if (functionToCall != null)
        {
            functionToCall(userId, messageContent);
        }
    }

    #region SendMessage

    public void SendSizeInfo()
    {
        var msg = CreateMessage((byte) HoloMessageType.ChangeSlider);
        msg.Write(10);
        _serverConnection.Broadcast(msg);
    }

    public void SendSliderValue(string msgContent)
    {
        var msg = CreateMessage((byte) HoloMessageType.ChangeSlider);
        msg.Write(msgContent);
        _serverConnection.Broadcast(msg);
    }

    public void SendDebugMessage()
    {
        Debug.Log("Send debug message to server...");

        string debugMsg = string.Format("{0} is alive!", LocalUserId);
        NetworkOutMessage msg = CreateMessage((byte) HoloMessageType.DebugMsg);
        msg.Write(debugMsg);
        _serverConnection.Broadcast(msg);
    }

    #endregion
}