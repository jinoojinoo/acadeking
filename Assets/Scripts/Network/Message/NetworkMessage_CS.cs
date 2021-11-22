using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using UnityEngine.Networking.Match;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class NetworkMessage_CS_UpdateChat : NetworkMessage_Receive<NetworkMessage_CS_UpdateChat>
{
    public NetworkMessage_CS_UpdateChat()
    {
        m_noneuiAction = true;
    }

    public uint NetID;
    public string chatmessage;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        NetID = MyNetworkManager.Instance.MyLobbyPlayer.netId.Value;
        chatmessage = string.Format("{0} : {1}", MyNetworkManager.Instance.MyLobbyPlayer.UserName, (string)parameters[0]);
        MyNetworkManager.Instance.client.Send((short)id, this);
    }
}

public class NetworkMessage_CharacterInfo : NetworkMessage_Receive<NetworkMessage_CharacterInfo>
{
    public NetworkMessage_CharacterInfo()
    {
        m_noneuiAction = true;
    }

    public uint NetId;
    public string UserName;
    public string BallList;
    public int RecordScore;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        NetId = MyNetworkManager.Instance.MyLobbyPlayer.netId.Value;
        UserName = AccountManager.Instance.GetUserName();
        BallList = AccountManager.Instance.MyBallList.SaveToString();
        RecordScore = AccountManager.Instance.MyAccountInfo.MyScore;
        MyNetworkManager.Instance.client.Send((short)id, this);
    }
}

public class NetworkMessage_ThrowBall : NetworkMessage_Receive<NetworkMessage_ThrowBall>
{
    public NetworkMessage_ThrowBall()
    {
        m_noneuiAction = true;
    }

    public uint netID;
    public int index;
    public double time;
    public Vector3 pos;
    public Vector3 power;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        netID = (uint)parameters[0];
        index = (int)parameters[1];
        time = (double)parameters[2];
        pos = (Vector3)parameters[3];
        power = (Vector3)parameters[4];

        Debug.LogError("netid " + netID + " , index : " + index + " , time : " + time + " , pos : " + pos + " , power : " + power);
        MyNetworkManager.Instance.client.Send((short)id, this);        
    }
}