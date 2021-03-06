using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
//using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

public partial class MyNetworkManager : NetworkManager
{
    private const int Room_Size = 10;

    private const string Network_IPAdress = "127.0.0.1";
    private const int NetWork_ServerPort = 7777;
    // Use this for initialization

    private static bool applicationIsQuitting = false;

    private static MyNetworkManager m_instance = null;
    public static MyNetworkManager Instance
    {
        get
        {
            if (applicationIsQuitting)
                return null;

            if (m_instance == null)
            {
                GameObject obj = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.NetworkManager);
                m_instance = obj.GetComponent<MyNetworkManager>();
                //                m_instance.autoCreatePlayer = false;
                GameObject.DontDestroyOnLoad(obj);
            }

            return m_instance;
        }
    }

    void OnApplicationQuit()
    {
        m_instance = null;
        applicationIsQuitting = true;
    }

    public static bool IsCreateInstance() { return m_instance != null; }

    public class CreateRoominfo
    {
        public string CurrentRoomName;
        public int MaxRound;
        public int MaxTime;

        public void Init(string roominfo)
        {
            string[] infos = roominfo.Split(',');
            CurrentRoomName = infos[0];
            MaxRound = int.Parse(infos[1]);
            MaxTime = int.Parse(infos[2]);
        }
        public void Init(string name, int round, int time)
        {
            CurrentRoomName = string.Format("{0}", name);
            MaxRound = round;
            MaxTime = time;
        }

        public int GetMaxRound
        {
            get
            {
                return MaxRound;
            }
        }

        public float GetRoundTime
        {
            get
            {
                return GlobalValue_Table.Instance.GetRoundTime(MaxTime);
            }
        }

        public string GetRoundString()
        {
            return string.Format("Round {0:0}", MaxRound + 1);
        }

        public string GetTimeString()
        {
            return string.Format("{0:00} Sec", GetRoundTime);
        }
    }

    private CreateRoominfo m_creatroomInfo = new CreateRoominfo();
    public CreateRoominfo CreateRoomInfo
    {
        get
        {
            return m_creatroomInfo;
        }
    }

    public bool IsHost
    {
        get
        {
            return NetworkServer.active;
        }
    }

    public System.Action<bool> LobbyPlayerReadyFunc
    {
        get;
        set;
    }

    public void OnLobbyServerPlayersReady()
    {
        if (NetworkServer.active == false)
            return;
        if (LobbyPlayerReadyFunc == null)
            return;

        Debug.LogError("OnLobbyServerPlayersReady");
        ConnectedPlayerList.RemoveAll(x => x == null);
        if (ConnectedPlayerList.Count <= 1)
        {
#if UNITY_EDITOR
            LobbyPlayerReadyFunc(true);
#else
            LobbyPlayerReadyFunc(false);
#endif
            return;
        }

        foreach(MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            if (player.m_readyToBegin == false)
            {
                LobbyPlayerReadyFunc(false);
                return;
            }
        }

        LobbyPlayerReadyFunc(true);
    }

    public bool CheckLobbyServerPlayersReady()
    {
        if (NetworkServer.active == false)
            return false;

        ConnectedPlayerList.RemoveAll(x => x == null);
        if (ConnectedPlayerList.Count <= 1)
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            if (player.m_readyToBegin == false)
            {
                return false;
            }
        }

        return true;
    }

    // server
    public void CheckLoadSceneComplete()
    {
        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            if (player.m_loadComplete != MyNetworkLobbyPlayer.SceneLoadState.Complete)
                return;
        }

        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            player.Rpc_OnLoadSceneDone();
        }
    }

    private bool m_checkComplete = false;
    private void ActionLoadSceneComplete()
    {
        if (MyLobbyPlayer.isServer && MyLobbyPlayer.IsLobbyLocal && m_checkComplete == false)
        {
            m_checkComplete = true;

            NetworkServer.Spawn(ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.Network_TimeManager));
//            NetworkServer.Spawn(ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.Network_WeaponManager));
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            if (player.m_loadComplete != MyNetworkLobbyPlayer.SceneLoadState.Load)
                return;
        }

        MyLobbyPlayer.m_loadComplete = MyNetworkLobbyPlayer.SceneLoadState.LoadInGame;
        m_checkComplete = false;
        base.ServerChangeScene(newSceneName);
    }       

    public void CheckInitComplete()
    {
        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            if (player.m_initComplete == false)
                return;
        }

        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            Debug.LogError("player : " + player + " , : Rpc_InitCompleteAll ");
            player.Rpc_InitCompleteAll();
        }

        ActionLoadSceneComplete();
    }

    //     public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    //     {
    //         GameObject obj = ResourceManager.Instance.GetGameObjectByTable(null, UIGAMEOBJECT_TYPE.MemberNetwork);
    //         return obj;
    //     }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.LogError("OnServerAddPlayer : " + UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name);
        base.OnServerAddPlayer(conn, playerControllerId);

        GameUISequence sequence = GameUIManager.Instance.GetCurrentUISequence();
        if (sequence == null)
            return;
        if (sequence.MyGameUIMode == GAME_UI_MODE.UI_MemberList)
            NetworkMessageManager.Instance.SendMessage(Network_ID.SC_MemberInfos);
    }

    /*    public override void OnLobbyClientConnect(NetworkConnection conn)
        {
            base.OnLobbyClientConnect(conn);
            ClientScene.AddPlayer(client.connection, 0);
        }*/

    public override void OnDropConnection(bool success, string extendedInfo)
    {
        base.OnDropConnection(success, extendedInfo);
        InGameUIScene.Instance.SetUICameraEnable(true);
    }

    private MyNetworkLobbyPlayer m_hostPlayer = null;
    public MyNetworkLobbyPlayer HostPlayer
    {
        get
        {
            if (m_hostPlayer == null)
                m_hostPlayer = m_connectedplayerList.Find(x => x.IsTheServerPlayer);

            return m_hostPlayer;
        }
    }

    private MyNetworkLobbyPlayer m_mylobbyPlayer = null;
    public MyNetworkLobbyPlayer MyLobbyPlayer
    {
        get
        {
            return m_mylobbyPlayer;
        }
        set
        {
            m_mylobbyPlayer = value;
            m_mylobbyPlayer.Reset();
        }
    }

    private MyNetworkGamePlayer m_myGamePlayer = null;
    public MyNetworkGamePlayer MyGamePlayer
    {
        get
        {
            return m_myGamePlayer;
        }
        set
        {
            m_myGamePlayer = value;
        }
    }

    public void InitInGame()
    {
        int count = ConnectedPlayerList.Count;
        int index = Random.Range(0, count);

        SumScore.Clear();
        MyScore.Clear();
        OtherScore.Clear();
        UnlistMatch();
        NetworkMessageManager.Instance.SendMessage(Network_ID.SC_StartInGame, ConnectedPlayerList.Count <= 0 ? 0 : ConnectedPlayerList[index].netId.Value);
    }

    public void ResetGame()
    {
        SumScore.Clear();
        MyScore.Clear();
        OtherScore.Clear();
    }

    public void UpdateStartTime(float updatetime)
    {
        foreach (MyNetworkLobbyPlayer player in ConnectedPlayerList)
        {
            if (player == null)
                continue;

            player.Rpc_UpdateStartCheckTime(updatetime);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////

    private List<MyNetworkLobbyPlayer> m_connectedplayerList = new List<MyNetworkLobbyPlayer>();
    public List<MyNetworkLobbyPlayer> ConnectedPlayerList
    {
        get
        {
            return m_connectedplayerList;
        }
    }

    private Dictionary<uint, MyNetworkGamePlayer> m_ingameplayerList = new Dictionary<uint, MyNetworkGamePlayer>();
    public Dictionary<uint, MyNetworkGamePlayer> InGamePlayerList
    {
        get
        {
            return m_ingameplayerList;
        }
    }

    private class SaveScoreIndex
    {
        private int m_current;
        private int m_old;
        public int Old
        {
            get
            {
                return m_old;
            }
        }
        public int Current
        {
            set
            {
                m_old = m_current;
                m_current = value;
            }
            get
            {
                return m_current;
            }
        }
        public int Value
        {
            get
            {
                int result = Current >= Old ? Current : Old;
                Current = result;
                return result;
            }
        }

        public void Clear()
        {
            m_current = 0;
            m_old = 0;
        }
    }
    private SaveScoreIndex SumScore = new SaveScoreIndex();
    private SaveScoreIndex MyScore = new SaveScoreIndex();
    private SaveScoreIndex OtherScore = new SaveScoreIndex();

    public bool GetGameResult()
    {
        return MyScore.Value > OtherScore.Value;
    }

    public void GetMultieplayCurrentScore(ref int sumscore, ref int myscore, ref int otherscore)
    {
        int sum = 0;
        foreach(MyNetworkGamePlayer player in InGamePlayerList.Values)
        {
            sum += player.GameScore;
            if (player.IsLocalGamePlayer)
                MyScore.Current = player.GameScore;
            else
                OtherScore.Current = player.GameScore;
        }

        SumScore.Current = sum;
        sumscore = SumScore.Value;
        myscore = MyScore.Value;
        otherscore = OtherScore.Value;
    }

    public void AddConnectedPlayer(MyNetworkLobbyPlayer player)
    {
        m_connectedplayerList.Add(player);
        OnLobbyServerPlayersReady();
    }

    public void RemoveConnectedPlayer(MyNetworkLobbyPlayer player)
    {
        m_connectedplayerList.Remove(player);
        OnLobbyServerPlayersReady();
    }

    public MyNetworkLobbyPlayer GetConnectedPlayer(uint netid)
    {
        return m_connectedplayerList.Find(x => x.netId.Value == netid);
    }

    public void AddInGamePlayer(MyNetworkGamePlayer player)
    {
        m_ingameplayerList.Add(player.LobbyNetID, player);
    }

    public void RemoveGamePlayer(MyNetworkGamePlayer player)
    {
        m_ingameplayerList.Remove(player.LobbyNetID);
    }

    public MyNetworkGamePlayer GetIngamePlayer(uint playerid)
    {
        if (m_ingameplayerList.ContainsKey(playerid) == false)
            return null;

        return m_ingameplayerList[playerid];
    }

    public bool IsInitComplete()
    {
        return m_connectedplayerList.Count == m_ingameplayerList.Count;
    }

    public static MyNetworkLobbyPlayer GetPlayerForConnection(NetworkConnection conn)
    {
        return conn.playerControllers[0].gameObject.GetComponent<MyNetworkLobbyPlayer>();
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        Debug.Log("OnServerRemovePlayer");
        base.OnServerRemovePlayer(conn, player);

        MyNetworkLobbyPlayer connectedPlayer = GetPlayerForConnection(conn);
        if (connectedPlayer != null)
        {
            Destroy(connectedPlayer);
            RemoveConnectedPlayer(connectedPlayer);
        }
    }

    public bool NoneReward = false;
    public void ExitInGame(System.Action func = null)
    {
        if (NetworkServer.active)
        {
            if (matchMaker != null && matchInfo != null)
            {
                matchMaker.DestroyMatch(matchInfo.networkId, 0, (success, info) =>
                {
                    if (!success)
                    {
                        Debug.LogErrorFormat("Failed to terminate matchmaking game. {0}", info);
                    }

                    StopMatchMaker();
                    StopHost();

                    matchInfo = null;

                    if (func != null)
                        func();
                });
            }
            else
            {
                Debug.LogWarning("No matchmaker or matchInfo despite being a server in matchmaking state.");

                StopMatchMaker();
                StopHost();
                matchInfo = null;

                if (func != null)
                    func();
            }
        }
        else
        {
            if (matchMaker != null && matchInfo != null)
            {
                matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, (success, info) =>
                {
                    if (!success)
                    {
                        Debug.LogErrorFormat("Failed to disconnect from matchmaking game. {0}", info);
                    }
                    StopMatchMaker();
                    StopClient();
                    matchInfo = null;

                    if (func != null)
                        func();
                });
            }
            else
            {
                Debug.LogWarning("No matchmaker or matchInfo despite being a client in matchmaking state.");

                StopMatchMaker();
                StopClient();
                matchInfo = null;

                if (func != null)
                    func();
            }
        }
    }

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        base.OnDestroyMatch(success, extendedInfo);

        if (!success)
        {
            Debug.LogErrorFormat("Failed to terminate matchmaking game. {0}", extendedInfo);
        }

        StopHost();
        matchInfo = null;
    }

    public void UnlistMatch()
    {
        if (matchMaker == null)
            return;

        matchMaker.SetMatchAttributes(matchInfo.networkId, false, 0, (success, info) => Debug.Log("Match hidden"));
    }

    public void ListMatch()
    {
        if (matchMaker == null)
            return;

        matchMaker.SetMatchAttributes(matchInfo.networkId, true, 0, (success, info) => Debug.Log("Match shown"));
    }

    public void OnDestroy()
    {
        if (m_instance == null)
            return;

        ExitInGame();
        ClearConnectPlayerList();
    }
}