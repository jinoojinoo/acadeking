using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking.Match;
using UnityEngine.Networking;

public partial class NetworkMessageManager : Singleton<NetworkMessageManager>
{
    public NetworkMessageManager()
    {
        UnityEngine.Networking.LogFilter.currentLogLevel = UnityEngine.Networking.LogFilter.Debug;
        InitMessage();
    }

    private Dictionary<Network_ID, NetworkMessage_Base> m_messageList = new Dictionary<Network_ID, NetworkMessage_Base>();
    public T GetMessageBase<T>(Network_ID id) where T : NetworkMessage_Base
    {
        if (m_messageList.ContainsKey(id) == false)
            return null;

        return m_messageList[id] as T;
    }

    private Dictionary<Network_ID, System.Action<NetworkMessage>> m_networkdeventList = new Dictionary<Network_ID, System.Action<NetworkMessage>>();

    public void SendMessage(Network_ID id, params object[] parameters)
    {
        if (m_messageList.ContainsKey(id) == false)
            return;

        if (id < Network_ID.SC_MAX && id > Network_ID.SC_Start && MyNetworkManager.Instance.IsHost == false)
            return;

        m_messageList[id].Send(id, parameters);
    }

    private void RegiesterMessageEvent<T>(Network_ID id, System.Action<bool> action) where T : NetworkMessage_Base, new()
    {
        T newbase = new T();

        newbase.ReceiveEventAction = action;
        m_messageList.Add(id, newbase);
    }

    private void RegiesterMessageEvent<T>(Network_ID id) where T : NetworkMessage_Base, new()
    {
        T newbase = new T();
        m_messageList.Add(id, newbase);
    }

    public void AddReceivePacketHandler(Network_ID id, System.Action<bool> action)
    {
        if (m_messageList.ContainsKey(id) == false)
            return;

        m_messageList[id].ReceiveEventAction -= action;
        m_messageList[id].ReceiveEventAction += action;
    }

    public void RemoveReceivePacketHandler(Network_ID id, System.Action<bool> action)
    {
        if (m_messageList.ContainsKey(id) == false)
            return;

        m_messageList[id].ReceiveEventAction -= action;
    }

    ////////////////////////

    private void RegiesterMessageEvent_SC<T>(Network_ID id, NetworkClient lobbyClient) where T : NetworkMessage_Base, new()
    {
        T newbase = new T();
        lobbyClient.RegisterHandler((short)id, newbase.OnMessageReceived);

        if (m_messageList.ContainsKey(id))
            m_messageList.Remove(id);
        m_messageList.Add(id, newbase);
    }

    private void RegiesterMessageEvent_CS<T>(Network_ID id) where T : NetworkMessage_Base, new()
    {
        T newbase = new T();
        NetworkServer.RegisterHandler((short)id, newbase.OnMessageReceived);

        if (m_messageList.ContainsKey(id))
            m_messageList.Remove(id);
        m_messageList.Add(id, newbase);
    }

    public void AddReceivePacketHandler(Network_ID id, System.Action<NetworkMessage_Base> action)
    {
        if (m_messageList.ContainsKey(id) == false)
            return;

        m_messageList[id].ReceiveEventForMessage -= action;
        m_messageList[id].ReceiveEventForMessage += action;
    }

    public void RemoveReceivePacketHandler(Network_ID id, System.Action<NetworkMessage_Base> action)
    {
        if (m_messageList.ContainsKey(id) == false)
            return;

        m_messageList[id].ReceiveEventForMessage -= action;
    }

}
