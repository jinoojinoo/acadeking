using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public enum Network_ID
{
    None = UnityEngine.Networking.MsgType.Highest + 1,
    ListMatch,
    CreateRoom,
    JoinRoom,

    SC_Start,
    SC_MemberInfos,
    SC_StartInGame,
    SC_ChangeScene,
    SC_PlayerMovement,
//    SC_SyncTime,
    SC_CharacterInfo,
    SC_ThrowBall,
    SC_ItemBuy,
    SC_ItemEquip,
    SC_MAX,

    CS_Start,
    CS_UpdateChat,
    CS_CharacterInfo,
    CS_ThrowBall,
    CS_ItemBuy,
    CS_ItemEquip,
    CS_MAX,

    Max,
}

public partial class NetworkMessageManager : Singleton<NetworkMessageManager>
{
    private void InitMessage()
    {
        m_messageList.Clear();

        RegiesterMessageEvent<NetworkMessage_ListMatch>(Network_ID.ListMatch, null);
        RegiesterMessageEvent<NetworkMessage_CreateRoom>(Network_ID.CreateRoom, null);
        RegiesterMessageEvent<NetworkMessage_JoinRoom>(Network_ID.JoinRoom, null);

        RegiesterMessageEvent_CS<NetworkMessage_CS_UpdateChat>(Network_ID.CS_UpdateChat);
        RegiesterMessageEvent_CS<NetworkMessage_CharacterInfo>(Network_ID.CS_CharacterInfo);
        RegiesterMessageEvent_CS<NetworkMessage_ThrowBall>(Network_ID.CS_ThrowBall);        
    }

    // s->c
    public void RegisterHandlerForClient(NetworkClient lobbyClient)
    {
        RegiesterMessageEvent_SC<NetworkMessage_SC_MemberInfos>(Network_ID.SC_MemberInfos, lobbyClient);
        RegiesterMessageEvent_SC<NetworkMessage_SC_StartInGame>(Network_ID.SC_StartInGame, lobbyClient);
        RegiesterMessageEvent_SC<NetworkMessage_SC_ChangeScene>(Network_ID.SC_ChangeScene, lobbyClient);
        RegiesterMessageEvent_SC<NetworkMessage_CharacterInfo>(Network_ID.SC_CharacterInfo, lobbyClient);
        RegiesterMessageEvent_SC<NetworkMessage_ThrowBall>(Network_ID.SC_ThrowBall, lobbyClient);
    }

    // c->s
    public void RegisterHandlerForServer()
    {
        NetworkServer.RegisterHandler((short)Network_ID.CS_CharacterInfo, ReceiveServer_CharacterInfo);
        NetworkServer.RegisterHandler((short)Network_ID.CS_ThrowBall, ReceiveServer_ThrowBall);
    }

    private void ReceivePacket_ListMatch(bool success)
    {

    }

    private void ReceiveServer_CharacterInfo(NetworkMessage _message)
    {
        NetworkMessage_CharacterInfo _msg = _message.ReadMessage<NetworkMessage_CharacterInfo>();
        NetworkServer.SendToAll((short)Network_ID.SC_CharacterInfo, _msg);
    }

    private void ReceiveServer_ThrowBall(NetworkMessage _message)
    {
        NetworkMessage_ThrowBall _msg = _message.ReadMessage<NetworkMessage_ThrowBall>();
        NetworkServer.SendToAll((short)Network_ID.SC_ThrowBall, _msg);
    }
}
