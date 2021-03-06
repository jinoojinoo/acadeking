using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
public partial class MyNetworkManager : NetworkManager
{
    private enum ClientActionKind
    {
        SceneChanged,
        Disconnenct,
        StopClient,
        MAX,
    }

    private Dictionary<ClientActionKind, Dictionary<string, System.Action>> m_onActionFunc = new Dictionary<ClientActionKind, Dictionary<string, Action>>();

    private Dictionary<string, System.Action> GetAction(ClientActionKind kind)
    {
        if (m_onActionFunc.ContainsKey(kind) == false)
            return null;

        return m_onActionFunc[kind];
    }

    private void ActionClient(ClientActionKind kind)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name;

        Dictionary<string, System.Action> list = GetAction(kind);
        if (list == null)
            return;

        if (list.ContainsKey(sceneName))
            list[sceneName]();
    }

    private void InitClientHandler()
    {
        m_onActionFunc.Clear();
        for (int i = 0; i < (int)ClientActionKind.MAX; ++i)
            m_onActionFunc.Add((ClientActionKind)i, new Dictionary<string, Action>());

        Dictionary<string, System.Action> list = GetAction(ClientActionKind.SceneChanged);
        list.Clear();
        list.Add(SceneManager.SceneName_Lobby, OnClientSceneChanged_Lobby);
        list.Add(SceneManager.SceneName_InGame, OnClientSceneChanged_InGame);

        list = GetAction(ClientActionKind.Disconnenct);
        list.Clear();
        list.Add(SceneManager.SceneName_Lobby, OnClientDisconnect_Lobby);
        list.Add(SceneManager.SceneName_EMPTY, OnClientDisconnect_Emtpy);
        list.Add(SceneManager.SceneName_InGame, OnClientDisconnect_InGame);

        list = GetAction(ClientActionKind.StopClient);
        list.Clear();
        list.Add(SceneManager.SceneName_Lobby, OnClientDisconnect_Lobby);
        list.Add(SceneManager.SceneName_EMPTY, OnClientDisconnect_Emtpy);
        list.Add(SceneManager.SceneName_InGame, OnClientDisconnect_InGame);
    }

    public override void OnStartClient(NetworkClient lobbyClient)
    {
        base.OnStartClient(lobbyClient);

        NetworkMessageManager.Instance.RegisterHandlerForClient(lobbyClient);
        InitClientHandler();

//        lobbyClient.RegisterHandler((short)Network_ID.SC_SyncTime, OnReceiveSyncTime);        
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        Debug.LogError("OnClientSceneChanged");
        base.OnClientSceneChanged(conn);

        ActionClient(ClientActionKind.SceneChanged);
    }

    private void OnClientSceneChanged_Lobby()
    {

    }

    private void OnClientSceneChanged_InGame()
    {
        foreach (MyNetworkLobbyPlayer player in m_connectedplayerList)
        {
            if (player == null)
                continue;

            player.OnEnterInGame();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.LogError("OnClientConnect");
        base.OnClientConnect(conn);

        GameUIManager.Instance.PushSequence(GAME_UI_MODE.UI_MemberList);
    }

    public override void OnStopClient()
    {
        Debug.LogError("OnStopClient");
        base.OnStopClient();

//        ClearConnectPlayerList();
        ActionClient(ClientActionKind.StopClient);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.LogError("OnClientDisconnect");
        base.OnClientDisconnect(conn);

  //      ClearConnectPlayerList();
        ActionClient(ClientActionKind.Disconnenct);
    }

    private void OnClientDisconnect_Lobby()
    {
        GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
        if (sequence == null)
            return;

        ClearConnectPlayerList();
        if (sequence.MyGameUIMode == GAME_UI_MODE.UI_RoomList)
        {

        }
        else if (sequence.MyGameUIMode == GAME_UI_MODE.UI_MemberList)
        {
            GameUIManager.Instance.PopSequence(GAME_UI_MODE.UI_Lobby);
        }
    }

    private void OnClientDisconnect_Emtpy()
    {
        Debug.LogError("OnClientDisconnect_Emtpy");
        ReturnToLobby();
    }

    private void OnClientDisconnect_InGame()
    {
        Debug.LogError("OnClientDisconnect_InGame");
        if (InGamePlayerList.Count <= 0)
        {
            if (InGameManager.Instance.GameState != InGameManager.GAME_STATE.ReturnToLobby)
                ReturnToLobby();

            return;
        }

        GAME_UI_MODE mode = GameUIManager.Instance.GetCurrentUISequence().MyGameUIMode;
        if (mode == GAME_UI_MODE.UI_GameResult)
        {
            if (InGameManager.Instance.GameState == InGameManager.GAME_STATE.ReturnToLobbyFromResult)
            {
                ReturnToLobby();
                return;
            }
        }
        else
        {
            if (MyNetworkManager.Instance.NoneReward)
            {
                MyNetworkManager.Instance.NoneReward = false;
                ReturnToLobby();
            }
            else
            {
                UI_GameResult.ShowGameResult(true);
            }
        }
    }

    public void ReturnToLobby()
    {
        Debug.LogError("ReturnToLobby");
        InGameManager.Instance.GameState = InGameManager.GAME_STATE.ReturnToLobby;
        PopupManager.Instance.AllClearPopup();
        ClearConnectPlayerList();
        SceneManager.Instance.LoadScene(SceneManager.SceneName_Lobby);
    }

    private void ClearConnectPlayerList()
    {
        foreach (MyNetworkGamePlayer player in InGamePlayerList.Values)
        {
            if (player == null)
                continue;

            Destroy(player.gameObject);
        }
        InGamePlayerList.Clear();
        MyNetworkManager.Instance.ConnectedPlayerList.Clear();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public double SyncServerTime
    {
        get
        {
            if (MyLobbyPlayer == null)
                return RealTime.time;

            return MyLobbyPlayer.GetServerTime();
        }
    }
}
