using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public abstract class NetworkMessage_Base : MessageBase
{
    protected bool m_noneuiAction = false;

    private System.Action<bool> m_receiveeventAction = null;
    public System.Action<bool> ReceiveEventAction
    {
        set
        {
            m_receiveeventAction = value;
        }
        get
        {
            return m_receiveeventAction;
        }
    }

    protected void Action_ReceiveEvent(bool success)
    {
        if (m_receiveeventAction == null)
            return;

        m_receiveeventAction(success);
    }

    private System.Action<NetworkMessage_Base> m_receiveeventforMessage = null;
    public System.Action<NetworkMessage_Base> ReceiveEventForMessage
    {
        set
        {
            m_receiveeventforMessage = value;
        }
        get
        {
            return m_receiveeventforMessage;
        }
    }

    protected void Action_ReceiveEventForMessage(NetworkMessage_Base message)
    {
        if (m_receiveeventforMessage == null)
            return;

        m_receiveeventforMessage(message);
    }

    protected NetworkMessageManager MessageManager
    {
        get;
        set;
    }

    public virtual void Send(Network_ID id, params object[] parameters)
    {
        if (m_noneuiAction == false)
            ActionMessageSendForSequece();

        SendMessage(id, parameters);
    }

    protected abstract void SendMessage(Network_ID id, params object[] parameters);
    
    protected void LogNetworkMessage(bool success, string extendedInfo)
    {
        ActionMessageRecevieForSequence();
        Debug.LogError("success : " + success + " , extendedInfo : " + extendedInfo);
    }

    protected void ActionMessageSendForSequece()
    {
        GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
        if (sequence == null)
            return;

        sequence.ActionMessageSend();
    }

    protected void ActionMessageRecevieForSequence()
    {
        GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
        if (sequence == null)
            return;
        sequence.ActionMessageReceive();
    }

    public virtual void OnMessageReceived(NetworkMessage netMessage)
    {

    }
}

public abstract class NetworkMessage_Receive<T> : NetworkMessage_Base where T : NetworkMessage_Base, new()
{
    public override void OnMessageReceived(NetworkMessage netMessage)
    {
/*        Debug.LogError("OnMessageReceived : " + this);*/
        ActionMessageRecevieForSequence();

        T objectMessage = netMessage.ReadMessage<T>();
        Action_ReceiveEventForMessage(objectMessage);
    }
}

public class NetworkMessage_ListMatch : NetworkMessage_Base
{
    private List<MatchInfoSnapshot> m_roomList = new List<MatchInfoSnapshot>();
    public List<MatchInfoSnapshot> RoomList
    {
        get
        {
            return m_roomList;
        }
    }

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        int page = (int)parameters[0];
        string findname = (string)parameters[1];

        if (MyNetworkManager.Instance.matchMaker == null)
            MyNetworkManager.Instance.StartMatchMaker();

        MyNetworkManager.Instance.matchMaker.ListMatches(page, 10, findname, true, 0, 0, OnMatchList);
    }

    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        LogNetworkMessage(success, extendedInfo);

        if (matches == null)
        {
            Debug.Log("null Match List returned from server");
            Action_ReceiveEvent(false);
            return;
        }

        if (success == false)
            Debug.LogError(extendedInfo);

        Debug.LogError("matches : " + matches.Count );
        if (matches.Count > 0)
            Debug.LogError(matches[0].currentSize + " / " + matches[0].maxSize);

        m_roomList.Clear();
        foreach (MatchInfoSnapshot match in matches)
        {
            m_roomList.Add(match);
        }

        Action_ReceiveEvent(success);
    }
}

public abstract class NetworkMessage_MatchInfo : NetworkMessage_Base
{
    private MatchInfo m_currentmatchInfo = new MatchInfo();
    public MatchInfo CurrentMatchInfo
    {
        get
        {
            return m_currentmatchInfo;
        }
    }
}

public class NetworkMessage_CreateRoom : NetworkMessage_MatchInfo
{
    public const int Min_Member = 4;
    public const int Max_Member = 6;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        string name = (string)parameters[0];

        if (MyNetworkManager.Instance.matchMaker == null)
            MyNetworkManager.Instance.StartMatchMaker();

        MyNetworkManager.Instance.matchMaker.CreateMatch(
            name,
            2,
            true,
            "", "", "", 0, 0,
            OnMatchCreate);
    }

    private void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        MyNetworkManager.Instance.OnMatchCreate(success, extendedInfo, matchInfo);

        LogNetworkMessage(success, extendedInfo);

        if (matchInfo == null)
        {
            Debug.Log("null Match List returned from server");
            Action_ReceiveEvent(false);
            return;
        }

        if (success == false)
            Debug.LogError(extendedInfo);

        GameUtil.DeepCopy(matchInfo, CurrentMatchInfo);
        Action_ReceiveEvent(success);
    }
}

public class NetworkMessage_JoinRoom : NetworkMessage_MatchInfo
{
    private long m_sendnetworkID = -1;
    public long SendNetworkID
    {
        get
        {
            return m_sendnetworkID;
        }
    }

    private List<long> m_badnetworkID = new List<long>();
    public List<long> BadNetworkID
    {
        get
        {
            return m_badnetworkID;
        }
    }

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        long networkId = (long)parameters[0];
        m_sendnetworkID = networkId;

        if (MyNetworkManager.Instance.matchMaker == null)
            MyNetworkManager.Instance.StartMatchMaker();

        MyNetworkManager.Instance.matchMaker.JoinMatch(
            (UnityEngine.Networking.Types.NetworkID)networkId,
            "", "", "", 0, 0,
            OnMatchJoined);
    }

    private void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        MyNetworkManager.Instance.OnMatchJoined(success, extendedInfo, matchInfo);

        LogNetworkMessage(success, extendedInfo);

        if (matchInfo == null)
        {
            Debug.Log("null Match List returned from server");
            Action_ReceiveEvent(false);
            return;
        }

        if (success == false)
            Debug.LogError(extendedInfo);

        GameUtil.DeepCopy(matchInfo, CurrentMatchInfo);
        Action_ReceiveEvent(success);
    }

    public void SetBadNetwork()
    {
        if (m_sendnetworkID == -1)
            return;

        if (BadNetworkID.Contains(m_sendnetworkID))
            return;

        BadNetworkID.Add(m_sendnetworkID);
        m_sendnetworkID = -1;
    }
}