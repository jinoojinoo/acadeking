using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class NetworkMessage_SC_MemberInfos : NetworkMessage_Receive<NetworkMessage_SC_MemberInfos>
{
    [System.Serializable]
    public class MemberInfo
    {
        public string UserName;
        public uint netid;
        public bool IsReady;
        public string BallList;
        public int RecordScore;

        public MemberInfo(string username, uint netid, bool ready, string balllist, int record)
        {
            this.UserName = username;
            this.netid = netid;
            this.IsReady = ready;
            this.BallList = balllist;
            this.RecordScore = record;
        }
    }

    protected List<MemberInfo> m_infoList = new List<MemberInfo>();
    public List<MemberInfo> InfoList
    {
        get
        {
            return m_infoList;
        }
    }

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        m_infoList.Clear();
        for (int i = 0; i < MyNetworkManager.Instance.ConnectedPlayerList.Count; ++i)
        {
            MyNetworkLobbyPlayer player = MyNetworkManager.Instance.ConnectedPlayerList[i] as MyNetworkLobbyPlayer;
            m_infoList.Add(new NetworkMessage_SC_MemberInfos.MemberInfo(
                player.UserName,
                player.netId.Value, 
                player.m_readyToBegin,
                player.BallListStr,
                player.RecordScore));
        }

        NetworkServer.SendToAll((short)id, this);
    }

    public override void Serialize(NetworkWriter writer)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, m_infoList);
        //        NetworkWriter nw = new NetworkWriter();
        writer.StartMessage((short)Network_ID.SC_MemberInfos);
        writer.WriteBytesFull(ms.ToArray());
        ms.Dispose();
        writer.FinishMessage();
    }

    public override void Deserialize(NetworkReader reader)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(reader.ReadBytesAndSize());
        m_infoList = (List<MemberInfo>)bf.Deserialize(ms);
        ms.Dispose();
        Debug.LogError(m_infoList.Count);
    }
}

public class NetworkMessage_SC_StartInGame : NetworkMessage_Receive<NetworkMessage_SC_StartInGame>
{
    public uint m_monsterNetID;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        m_monsterNetID = (uint)parameters[0];
        NetworkServer.SendToAll((short)id, this);
    }
}

public class NetworkMessage_SC_ChangeScene : NetworkMessage_Receive<NetworkMessage_SC_ChangeScene>
{
    public string m_sceneName;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        m_sceneName = (string)parameters[0];

        NetworkServer.SendToAll((short)id, this);
    }
}
/*
public class NetworkMessage_SC_SyncTime : NetworkMessage_Receive<NetworkMessage_SC_SyncTime>
{
    public NetworkMessage_SC_SyncTime()
    {
        m_noneuiAction = true;
    }

    public double timeStamp;

    protected override void SendMessage(Network_ID id, params object[] parameters)
    {
        timeStamp = (double)parameters[0];

        NetworkServer.SendToAll((short)id, this);
    }
}*/