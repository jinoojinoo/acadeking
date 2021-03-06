using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class NetworkPlayerInfos
{
    public string CharacterName;
}

public class MyNetworkLobbyPlayer : NetworkBehaviour
{
    private System.Action m_RefreshLobbyPlayerFunc = null;
    public System.Action RefreshLobbyPlayerFunc
    {
        get
        {
            return m_RefreshLobbyPlayerFunc;
        }
        set
        {
            m_RefreshLobbyPlayerFunc = value;
            if (m_RefreshLobbyPlayerFunc != null)
                m_RefreshLobbyPlayerFunc();
        }
    }

    private UI_MemberList m_memberlist = null;
    private UI_MemberList MemberList
    {
        get
        {
            if (m_memberlist == null)
            {
                m_memberlist = GameUIManager.Instance.GetCurrentUISequence() as UI_MemberList;
            }
            return m_memberlist;
        }
    }

    private NetworkPlayerInfos m_mynetworkplayerInfo = new NetworkPlayerInfos();
    public NetworkPlayerInfos MyNetworkPlayerInfo
    {
        get
        {
            return m_mynetworkplayerInfo;
        }
    }

    public bool IsLobbyLocal
    {
        get
        {
            return MyNetworkManager.Instance.MyLobbyPlayer.netId.Value == this.netId.Value;
        }
    }

    [SyncVar]
    public string BallListStr;
    [SyncVar]
    public int RecordScore = 0;

    [SyncVar]
    public string UserName;

    public uint PlayerID
    {
        get
        {
            return netId.Value;
        }
    }

    [SyncVar(hook = "OnReadyToBegin")]
    public bool m_readyToBegin = false;

    [SyncVar]
    public bool IsTheServerPlayer = false;

    private SyncListString m_chatsyncList = new SyncListString();

//    [SyncVar(hook = "OnStartCheckTime")]
    public float m_startcheckTime = -1.0f;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        if (isLocalPlayer)
        {
            CmdRequestTime();
        }

        if (isLocalPlayer && isServer)
        {
            IsTheServerPlayer = true;
        }
    }

    public bool isNetworkTimeSynced = false;
    // timestamp received from server
    public double networkTimestamp;
    // server to client delay
    private double networkTimestampDelayMS;
    // when did we receive timestamp from server
    private double timeReceived;

    [Command]
    private void CmdRequestTime()
    {
        int timestamp = 0;
        if (isServer && isLocalPlayer)
            timestamp = NetworkTransport.GetNetworkTimestamp();
        else
            timestamp = (int)MyNetworkManager.Instance.HostPlayer.networkTimestamp;

        RpcNetworkTimestamp(timestamp);
    }

    [ClientRpc]
    private void RpcNetworkTimestamp(int timestamp)
    {
        isNetworkTimeSynced = true;
        networkTimestamp = timestamp;
        timeReceived = RealTime.time;

        // if client is a host, assume that there is 0 delay
        if (isServer)
        {
            networkTimestampDelayMS = 0;
        }
        else
        {
            byte error;
            networkTimestampDelayMS = NetworkTransport.GetRemoteDelayTimeMS
                (
                MyNetworkManager.Instance.client.connection.hostId,
                MyNetworkManager.Instance.client.connection.connectionId,
                timestamp,
                out error
                );
        }
    }

    public double GetServerTime()
    {
        double time = networkTimestamp + (networkTimestampDelayMS / 1000f) + ((RealTime.time - timeReceived));
        return time;
    }

#if UNITY_EDITOR
    private void LateUpdate()
    {
//        Debug.LogError(" if (isLocalPlayer) : " + isLocalPlayer + " , networkTimestamp : " + networkTimestamp + " , networkTimestampDelayMS : " + networkTimestampDelayMS + " , timeReceived + " + timeReceived + " , GetServerTime : " + GetServerTime());
    }
#endif

    public void Reset()
    {
        m_startcheckTime = -1.0f;
    }

    [ClientRpc]
    public void Rpc_UpdateStartCheckTime(float value)
    {
        m_startcheckTime = value;
        Debug.Log("update m_startcheckTime:" + value);
        RefreshPlayerInfo();
    }

    [Command]
    public void CmdClickReady()
    {
        m_readyToBegin = !m_readyToBegin;
    }

    public void OnReadyToBegin(bool ready)
    {
        m_readyToBegin = ready;
        RefreshPlayerInfo();

        if (NetworkServer.active)
            MyNetworkManager.Instance.OnLobbyServerPlayersReady();
    }

    //////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////
    public enum SceneLoadState
    {
        None,
        Wait,

        LoadInGame,
        Load,
        Complete,
        Done,
//        Ended,
    }
    [SyncVar(hook = "OnSceneLoadState")]
    public SceneLoadState m_loadComplete =  SceneLoadState.None;

    public void OnLoadSceneState(SceneLoadState state)
    {
        if (isLocalPlayer == false)
            return;

        Cmd_OnLoadSceneState(state);
    }

    [Command]
    public void Cmd_OnLoadSceneState(SceneLoadState state)
    {
        m_loadComplete = state;

        if (NetworkServer.active)
            MyNetworkManager.Instance.CheckLoadSceneComplete();
    }

    [ClientRpc]
    public void Rpc_OnLoadSceneDone()
    {
        m_loadComplete = SceneLoadState.Done;
    }

    private void OnSceneLoadState(SceneLoadState state)
    {
        m_loadComplete = state;

        if (this.isLocalPlayer && state == SceneLoadState.Wait)
        {
            InGameUIScene.Instance.ChangeScene(SceneManager.SceneName_InGame);
        }

        CheckInGamePlay();
    }

    //////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////
    [SyncVar(hook = "OnInitComplete")]
    public bool m_initComplete = false;
    [Command]
    public void Cmd_InitComplete()
    {
        m_initComplete = true;

        if (NetworkServer.active)
            MyNetworkManager.Instance.CheckInitComplete();
    }

    [ClientRpc]
    public void Rpc_InitCompleteAll()
    {
        if (isLocalPlayer == false)
            return;

        CheckInGamePlay();
    }

    public void CheckInitComplete()
    {
        if (MyNetworkManager.Instance.IsInitComplete() == false)
            return;

        MyNetworkManager.Instance.MyLobbyPlayer.Cmd_InitComplete();
    }

    private void OnInitComplete(bool complete)
    {
        m_initComplete = complete;
    }

    public void CheckInGamePlay()
    {
        if (isLocalPlayer == false)
            return;

        MyNetworkManager.Instance.SetConnectCount();
    }

    //////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////

    void ChatChanged(SyncListString.Operation op, int itemIndex)
    {
        Debug.Log("buf changed:" + op);

        if (MemberList == null)
            return;
        if (m_chatsyncList.Count < 1)
            return;

        MemberList.UpdateChatMessage(m_chatsyncList[m_chatsyncList.Count - 1]);
    }

    private void CS_UpdateChat(NetworkMessage_Base netMessage)
    {
        if (this.hasAuthority == false)
            return;

        NetworkMessage_CS_UpdateChat objectMessage = netMessage as NetworkMessage_CS_UpdateChat;
        m_chatsyncList.Add(objectMessage.chatmessage);
    }

    [Client]
    public override void OnStartClient()
    {
        Debug.LogError("Client Network Player start");
        base.OnStartClient();        
        MyNetworkManager.Instance.AddConnectedPlayer(this);

        //////////////////////////////////////

        m_chatsyncList.Callback = ChatChanged;
        NetworkMessageManager.Instance.AddReceivePacketHandler(Network_ID.CS_UpdateChat, CS_UpdateChat);
        if (MemberList != null)
            MemberList.InitItem(this);

        RefreshPlayerInfo();
    }

    public void ClientOnlyPlay_StartClient()
    {
        Debug.LogError("Client Network Player start");
        base.OnStartClient();
        MyNetworkManager.Instance.AddConnectedPlayer(this);

        //////////////////////////////////////

        m_chatsyncList.Callback = ChatChanged;
        NetworkMessageManager.Instance.AddReceivePacketHandler(Network_ID.CS_UpdateChat, CS_UpdateChat);
        if (MemberList != null)
            MemberList.InitItem(this);

        RefreshPlayerInfo();
    }

    public override void OnStartLocalPlayer()
    {
        Debug.LogError("OnStartLocalPlayer");
        base.OnStartLocalPlayer();

        RefreshPlayerInfo();
        MyNetworkManager.Instance.MyLobbyPlayer = this;
        MyNetworkManager.Instance.MyLobbyPlayer.UserName = AccountManager.Instance.GetUserName();
        MyNetworkManager.Instance.MyLobbyPlayer.BallListStr = AccountManager.Instance.MyBallList.SaveToString();
        MyNetworkManager.Instance.MyLobbyPlayer.RecordScore = AccountManager.Instance.MyAccountInfo.MyScore;

        if (MemberList != null)
            MemberList.InitLocalPlayer(this);        
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        Debug.Log("Client Network Player OnNetworkDestroy");

        if (MyNetworkManager.Instance != null)
            MyNetworkManager.Instance.RemoveConnectedPlayer(this);

        if (NetworkMessageManager.Instance != null)
            NetworkMessageManager.Instance.RemoveReceivePacketHandler(Network_ID.CS_UpdateChat, CS_UpdateChat);
    }

    public void RefreshPlayerInfo()
    {
        if (m_RefreshLobbyPlayerFunc == null)
            return;

        m_RefreshLobbyPlayerFunc();
    }

    private void OnDestroy()
    {
        if (m_memberlist == null)
            return;

        m_memberlist.DestroyItem(this.netId.Value);
    }

    [Client]
    public void OnEnterInGame()
    {
        if (hasAuthority == false)
            return;

        CmdClientReadyInGameScene();
    }

    [Command]
    private void CmdClientReadyInGameScene()
    {
        GameObject playerbaseobject = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.Network_InGamePlayer);
        MyNetworkGamePlayer player = playerbaseobject.GetComponent<MyNetworkGamePlayer>();
        player.InitGamePlayer(this);

        NetworkServer.SpawnWithClientAuthority(playerbaseobject, connectionToClient);       
    }
}
