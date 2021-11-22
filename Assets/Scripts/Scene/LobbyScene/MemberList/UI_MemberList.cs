using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class UI_MemberList : GameUISequence
{
    public UIGrid Grid;
    private int m_creatCount = 0;
    public UIButton StartButton;
    public UIButton ReadyButton;
    public UITextList ChatList;

    public UILabel SubjectLabel;
    public UILabel RoundLabel;
    public UILabel TimeLabel;

    private Dictionary<uint, UI_MemberList_Item> m_memberitemList = new Dictionary<uint, UI_MemberList_Item>();

    public UILabel StartLabel;
    public UILabel ReadyLabel;
    private bool IsReady
    {
        set
        {
            if (value)
                ReadyLabel.text = "Ready";
            else
                ReadyLabel.text = "Wait";
        }
    }

    private void Start()
    {
        AddPacketHandler(Network_ID.SC_MemberInfos, SC_MemerInfos);
        AddPacketHandler(Network_ID.SC_StartInGame, SC_StartInGame);
        AddPacketHandler(Network_ID.SC_ChangeScene, SC_ChangeScene);
        AddPacketHandler(Network_ID.SC_CharacterInfo, SC_CharacterInfo);
//        AddPacketHandler(Network_ID.CS_UpdateChat, CS_UpdateChat);

        MyNetworkManager.Instance.LobbyPlayerReadyFunc -= InitButton;
        MyNetworkManager.Instance.LobbyPlayerReadyFunc += InitButton;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();


        if (MyNetworkManager.Instance != null)
        {
            MyNetworkManager.Instance.LobbyPlayerReadyFunc = null;
            if (MyNetworkManager.Instance.MyLobbyPlayer != null)
                MyNetworkManager.Instance.MyLobbyPlayer.RefreshLobbyPlayerFunc = null;
        }
    }

    private void SC_MemerInfos(NetworkMessage_Base netMessage)
    {
        NetworkMessage_SC_MemberInfos objectMessage = netMessage as NetworkMessage_SC_MemberInfos;

        foreach (MyNetworkLobbyPlayer player in MyNetworkManager.Instance.ConnectedPlayerList)
        {
            NetworkMessage_SC_MemberInfos.MemberInfo info = objectMessage.InfoList.Find(x => x.netid == player.netId.Value);
            if (info == null)
                continue;

            player.RefreshPlayerInfo();
            player.UserName = info.UserName;
            player.BallListStr = info.BallList;
            player.RecordScore = info.RecordScore;
        }
    }

    private void SC_StartInGame(NetworkMessage_Base netMessage)
    {
        InGameManager.Instance.CurrentPlayMode = InGameManager.PLAY_MODE.Multiple;
        MyNetworkManager.Instance.MyLobbyPlayer.m_startcheckTime = 5.0f;

        MyNetworkManager.Instance.MyLobbyPlayer.RefreshPlayerInfo();
        MyNetworkManager.Instance.ResetGame();
    }

    private void SC_ChangeScene(NetworkMessage_Base netMessage)
    {
        NetworkMessage_SC_ChangeScene objectMessage = netMessage as NetworkMessage_SC_ChangeScene;
        MyNetworkManager.Instance.MyLobbyPlayer.Cmd_OnLoadSceneState(MyNetworkLobbyPlayer.SceneLoadState.Wait);
    }

    private void SC_CharacterInfo(NetworkMessage_Base netMessage)
    {
        NetworkMessage_CharacterInfo msg = netMessage as NetworkMessage_CharacterInfo;

        MyNetworkLobbyPlayer mylobbyplayer = MyNetworkManager.Instance.MyLobbyPlayer;
        if (mylobbyplayer.netId.Value == msg.NetId)
            return;

        MyNetworkLobbyPlayer player = MyNetworkManager.Instance.ConnectedPlayerList.Find(x => x.netId.Value == msg.NetId);
        if (player == null)
            return;

        Debug.LogError("msg:" + msg.BallList);

        player.UserName = msg.UserName;
        player.BallListStr = msg.BallList;
        player.RecordScore = msg.RecordScore;
        m_memberitemList[msg.NetId].Refresh();
    }

//     private void CS_UpdateChat(NetworkMessage_Base netMessage)
//     {
//         NetworkMessage_CS_UpdateChat objectMessage = netMessage as NetworkMessage_CS_UpdateChat;
// 
//         if (m_memberitemList.ContainsKey(objectMessage.NetID) == false)
//             return;
// 
// //        m_memberitemList[objectMessage.NetID].Talk();        
//     }

    public override void StartGameSequence(int option)
    {
        SubjectLabel.text = string.Format(StringTable.RoomName, MyNetworkManager.Instance.CreateRoomInfo.CurrentRoomName);
        RoundLabel.text = MyNetworkManager.Instance.CreateRoomInfo.GetRoundString();
        TimeLabel.text = MyNetworkManager.Instance.CreateRoomInfo.GetTimeString();

        InitLocalPlayer(MyNetworkManager.Instance.MyLobbyPlayer);
        InitMemberList();
        InitButton(false);

        StartButton.gameObject.SetActive(NetworkServer.active);
    }

    private void CreateCharacter()
    {
        NetworkMessageManager.Instance.SendMessage(Network_ID.CS_CharacterInfo);
    }

    private void Refresh()
    {
        IsReady = MyNetworkManager.Instance.MyLobbyPlayer.m_readyToBegin;
        float checktime = MyNetworkManager.Instance.MyLobbyPlayer.m_startcheckTime;

        StartButton.isEnabled = (checktime == -1) && MyNetworkManager.Instance.CheckLobbyServerPlayersReady();
        ReadyButton.isEnabled = (checktime == -1);

        if (checktime != -1)
        {
            if (NetworkServer.active)
                StartLabel.text = checktime.ToString();
            else
                ReadyLabel.text = checktime.ToString();
        }
    }

    public override void PopSequence(GAME_UI_MODE mode = GAME_UI_MODE.None)
    {
        MyNetworkManager.Instance.ExitInGame();
        MyNetworkManager.Instance.MyLobbyPlayer.RefreshLobbyPlayerFunc -= Refresh;
    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        m_creatCount = 0;
        foreach(UI_MemberList_Item item in m_memberitemList.Values)
        {
            if (item == null)
                continue;

            Destroy(item.gameObject);
        }
        m_memberitemList.Clear();

        return 0;
    }

    private void InitMemberList(int index = -1)
    {
        if (index == -1)
        {
            foreach(MyNetworkLobbyPlayer player in MyNetworkManager.Instance.ConnectedPlayerList)
                InitItem(player);
        }
        else
        {
            //            InitItem(index);
        }
    }

    public void InitButton(bool enabled)
    {
        StartButton.isEnabled = enabled;
    }

    public void InitLocalPlayer(MyNetworkLobbyPlayer player)
    {
        if (player == null)
            return;

        CreateCharacter();
        MyNetworkManager.Instance.MyLobbyPlayer.RefreshLobbyPlayerFunc -= Refresh;
        MyNetworkManager.Instance.MyLobbyPlayer.RefreshLobbyPlayerFunc += Refresh;
    }

    public void InitItem(MyNetworkLobbyPlayer player)
    {
        if (m_memberitemList.ContainsKey(player.netId.Value))
        {
            Grid.Reposition();
            return;
        }

        GameObject obj = ResourceManager.Instance.GetGameObjectByTable(Grid.transform, UIGAMEOBJECT_TYPE.MemberItem);
        obj.name = string.Format("{0:000}", m_creatCount++);

        UI_MemberList_Item item = null;
        item = obj.GetComponent<UI_MemberList_Item>();
        item.Init(player);

        m_memberitemList.Add(player.netId.Value, item);
        Grid.Reposition();

        InitButton(false);
    }

    public void DestroyItem(uint netid)
    {
        if (m_memberitemList.ContainsKey(netid) == false)
            return;

        m_memberitemList[netid].gameObject.transform.parent = null;
        Destroy(m_memberitemList[netid].gameObject);
        Grid.Reposition();
    }

    public void OnClick_StartButton()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);
        Debug.LogError("OnClick_StartButton");

        MyNetworkManager.Instance.InitInGame();

        StopCoroutine("CountForInGame");
        StartCoroutine("CountForInGame");
    }

    public void OnClick_ReadyButton()
    {
        SoundManager.Instance.PlaySound(UISOUND_ID.Button_Click);
        MyNetworkManager.Instance.MyLobbyPlayer.CmdClickReady();
    }

    private IEnumerator CountForInGame()
    {
        float count = 5;
        float currentTime = Time.realtimeSinceStartup;
        int checktime = -1;

        while (true)
        {
            count -= (Time.realtimeSinceStartup - currentTime);
            currentTime = Time.realtimeSinceStartup;
            if (checktime == (int)count)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            SoundManager.Instance.PlaySound(UISOUND_ID.Roll);
            checktime = (int)count;
            MyNetworkManager.Instance.UpdateStartTime(checktime);
            Debug.LogError("count : " + checktime);
            if (count < 0)
                break;
        }

        NetworkMessageManager.Instance.SendMessage(Network_ID.SC_ChangeScene, SceneManager.SceneName_InGame);
        yield break;
    }

    public void UpdateChatMessage(string msg)
    {
        ChatList.Add(msg);
    }
}