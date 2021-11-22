using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public partial class MyNetworkGamePlayer : NetworkBehaviour
{
    private Transform m_myTrans = null;
    private Transform MyTrans
    {
        get
        {
            if (m_myTrans == null)
                m_myTrans = this.transform;
            return m_myTrans;
        }
    }

    [Command]
    public void CmdAddScore(int score, bool isclean)
    {
        int check = score;
        if (isclean)
            check |= (1 << 16);

        CheckScore = check;
    }


    [SyncVar(hook = "SetCheckGameScore")]
    public int CheckScore = 0;

    DelegateSecrueProperty<int> m_encryption_score = new DelegateSecrueProperty<int>();
    public int GameScore
    {
        get
        {
            return m_encryption_score.Value;
        }
        set
        {
            m_encryption_score.Value = value;
        }
    }
    public bool IsClean;

    [SyncVar]
    public string CharacterName;
    [SyncVar(hook = "InitOK")]
    public bool m_initOK = false;
    [SyncVar]
    public string BallListStr;
    [SyncVar]
    public int RecordScore;

    private List<BallInfos> m_ballList;
    public List<BallInfos> BallList
    {
        get
        {
            if (m_ballList == null)
                m_ballList = AccountManager.BallInfosList.CreateFromJSON(BallListStr).m_ballList;

            return m_ballList;
        }
        set
        {
            m_ballList = value;
        }
    }

    [SyncVar]
    public float Start1Position = 0.0f;
    [SyncVar]
    public float Start2Position = 0.0f;
    [SyncVar]
    public float Start3Position = 0.0f;
    [SyncVar]
    public float Start4Position = 0.0f;
    [SyncVar]
    public float Start5Position = 0.0f;

    private List<float> m_ballpositionList;
    public List<float> BallPositionList
    {
        get
        {
            if (m_ballpositionList == null)
            {
                m_ballpositionList = new List<float>();
                m_ballpositionList.Add(Start1Position);
                m_ballpositionList.Add(Start2Position);
                m_ballpositionList.Add(Start3Position);
                m_ballpositionList.Add(Start4Position);
                m_ballpositionList.Add(Start5Position);
            }

            return m_ballpositionList;
        }
    }

    public bool IsLocalGamePlayer
    {
        get
        {
            return MyNetworkManager.Instance.MyLobbyPlayer.netId.Value == LobbyNetID;
        }
    }

    [SyncVar]
    public uint LobbyNetID = 0;

    [SyncVar]
    private bool m_readyforGame = false;
    public bool ReadyForGame
    {
        get
        {
            return m_readyforGame;
        }
    }

    [Command]
    public void Cmd_ReadyForGame()
    {
        m_readyforGame = true;
    }

    public override void OnStartClient()
    {
        InitClient();
    }

    [SyncVar]
    public bool m_checknextRound = false;
    [Command]
    public void Cmd_NextRound()
    {
        m_checknextRound = true;
        CheckAllNextRound();
    }

    private void CheckAllNextRound()
    {
        if (isServer == false)
            return;

        foreach (MyNetworkGamePlayer player in MyNetworkManager.Instance.InGamePlayerList.Values)
        {
            if (player.m_checknextRound == false)
                return;
        }

        foreach (MyNetworkGamePlayer player in MyNetworkManager.Instance.InGamePlayerList.Values)
        {
            player.m_checknextRound = false;
        }

        NetworkTimeManager.Instance.NextRound();
    }

    private void InitClient()
    { 
        MyNetworkManager.Instance.AddInGamePlayer(this);
        ArcadeKingManager.Instance.CreateBasketBall((int)LobbyNetID, BallList, BallPositionList, IsLocalGamePlayer);

        if (isServer)
        {
            Debug.LogError("server");
        }

        // complete - create character 
        MyNetworkManager.Instance.MyLobbyPlayer.CheckInitComplete();
        // complete - scene load
        MyNetworkManager.Instance.MyLobbyPlayer.OnLoadSceneState(MyNetworkLobbyPlayer.SceneLoadState.Complete);

        if (IsLocalGamePlayer)
        {
            MyNetworkManager.Instance.MyGamePlayer = this;
            RegisterNetworkMessages();
        }

//        ArcadeKingManager.Instance.GameScoreFunc -= SetGameScore;
//        ArcadeKingManager.Instance.GameScoreFunc += SetGameScore;
    }

    private void RegisterNetworkMessages()
    {
        NetworkMessageManager.Instance.AddReceivePacketHandler(Network_ID.SC_ThrowBall, OnReceiveThrowBall);
    }

    private void OnReceiveThrowBall(NetworkMessage_Base _message)
    {
        NetworkMessage_ThrowBall _msg = _message as NetworkMessage_ThrowBall;
        if (MyNetworkManager.Instance.InGamePlayerList.ContainsKey(_msg.netID) == false)
            return;

        MyNetworkGamePlayer player = MyNetworkManager.Instance.InGamePlayerList[_msg.netID];
        if (player.IsLocalGamePlayer)
            return;

        Ball ball = ArcadeKingManager.Instance.GetBall((int)_msg.netID, _msg.index);
        if (ball == null)
            return;

        ball.transform.localPosition = _msg.pos;
        ball.InitVelocity(_msg.power);
    }

    //[s->c]
    public void InitGamePlayer(MyNetworkLobbyPlayer player)
    {
        Debug.LogError("InitGamePlayer");

        LobbyNetID = player.netId.Value;

        m_initOK = true;

        CharacterName = player.UserName;
        BallListStr = player.BallListStr;
        GameScore = 0;
        RecordScore = player.RecordScore;

        Start1Position = UnityEngine.Random.Range(-1.0f, 1.0f);
        Start2Position = UnityEngine.Random.Range(-1.0f, 1.0f);
        Start3Position = UnityEngine.Random.Range(-1.0f, 1.0f);
        Start4Position = UnityEngine.Random.Range(-1.0f, 1.0f);
        Start5Position = UnityEngine.Random.Range(-1.0f, 1.0f);
    }    

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();

        if (MyNetworkManager.Instance != null)
            MyNetworkManager.Instance.RemoveGamePlayer(this);

        if (NetworkMessageManager.Instance != null)
            NetworkMessageManager.Instance.RemoveReceivePacketHandler(Network_ID.SC_ThrowBall, OnReceiveThrowBall);
    }

    private void InitOK(bool init)
    {
        m_initOK = init;
    }

    public void SetCheckGameScore(int score)
    {
        CheckScore = score;

        IsClean = (score >> 16 & 0x1) == 1;
        int current_score = GameScore + ((score & 0xffff) * (IsClean ? 2 : 1));

        RpcGameScore(current_score);
    }

    [ClientRpc]
    private void RpcGameScore(int score)
    {
        GameScore = score;
        if (ArcadeKingManager.Instance.MultieGameScoreFunc != null)
            ArcadeKingManager.Instance.MultieGameScoreFunc();
    }
}