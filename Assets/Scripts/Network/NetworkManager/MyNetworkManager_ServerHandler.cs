using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public partial class MyNetworkManager : NetworkManager
{
    private enum ServerActionKind
    {
        ServerDisconnect,
        ServerChange,
        MAX,
    }

    private Dictionary<ServerActionKind, Dictionary<string, System.Action>> m_onserverActionFunc = new Dictionary<ServerActionKind, Dictionary<string, System.Action>>();
    private Dictionary<string, System.Action> GetServerAction(ServerActionKind kind)
    {
        if (m_onserverActionFunc.ContainsKey(kind) == false)
            return null;

        return m_onserverActionFunc[kind];
    }

    private void ActionServer(ServerActionKind kind)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name;

        Dictionary<string, System.Action> list = GetServerAction(kind);
        if (list == null)
            return;

        if (list.ContainsKey(sceneName))
            list[sceneName]();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        networkSceneName = string.Empty;

        NetworkMessageManager.Instance.RegisterHandlerForServer();
        InitServerHandler();
    }

    private void InitServerHandler()
    {
        m_onserverActionFunc.Clear();
        for (int i = 0; i < (int)ServerActionKind.MAX; ++i)
            m_onserverActionFunc.Add((ServerActionKind)i, new Dictionary<string, System.Action>());

        Dictionary<string, System.Action> list = GetServerAction(ServerActionKind.ServerChange);
        list.Clear();
        list.Add(SceneManager.SceneName_Lobby, OnServerSceneChanged_Lobby);
        list.Add(SceneManager.SceneName_InGame, OnServerSceneChanged_InGame);

        list = GetServerAction(ServerActionKind.ServerDisconnect);
        list.Clear();
        list.Add(SceneManager.SceneName_Lobby, OnServerDisconnect_Lobby);
        list.Add(SceneManager.SceneName_EMPTY, OnServerDisconnect_Empty);
        list.Add(SceneManager.SceneName_InGame, OnServerDisconnect_InGame);
    }

    public override void OnStopServer()
    {
        Debug.Log("OnStopServer");
        base.OnStopServer();

        networkSceneName = string.Empty;

        for (int i = 0; i < m_connectedplayerList.Count; ++i)
        {
            MyNetworkLobbyPlayer player = m_connectedplayerList[i];
            if (player != null)
            {
                NetworkServer.Destroy(player.gameObject);
            }
        }
        m_connectedplayerList.Clear();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("---- Server send syncTime to client : " + conn.connectionId);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.LogError("OnServerSceneChanged");

        base.OnServerSceneChanged(sceneName);
        ActionServer(ServerActionKind.ServerChange);
    }

    private void OnServerSceneChanged_Lobby()
    {

    }

    private void OnServerSceneChanged_InGame()
    {

    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        ActionServer(ServerActionKind.ServerDisconnect);
    }

    private void OnServerDisconnect_Lobby()
    {
        GameUIManager.Instance.PopSequence(GAME_UI_MODE.UI_Lobby);
    }

    private void OnServerDisconnect_Empty()
    {
        if (MyNetworkManager.Instance.InGamePlayerList.Count <= 1)
            PopupManager.Instance.ShowPopup(POPUP_TYPE.ReturnLobby, OnClickOk);
    }

    private void OnServerDisconnect_InGame()
    {
        if (InGameManager.Instance.GameState!= InGameManager.GAME_STATE.Play)
        {
            MyNetworkManager.Instance.ExitInGame();
            return;
        }

        if (MyNetworkManager.Instance.InGamePlayerList.Count > 1)
            return;

        UI_GameResult.ShowGameResult(false, OnErrorOK);
    }

    private void OnClickOk(params object[] paramters)
    {
        MyNetworkManager.Instance.ExitInGame();
    }

    private void OnErrorOK()
    {
        MyNetworkManager.Instance.ExitInGame();
    }
}