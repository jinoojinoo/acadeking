using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking.Match;

public class UI_RoomList : GameUISequence
{
    public UI_RoomList_ScrollView m_ScrollView;

    public UIButton BackPageButton;
    public UIButton NextPageButton;
    public UIInput FindNameInput;

    public UILabel PageLabel;

    private bool m_sendnextpage = false;
    private bool SendNextPage
    {
        set
        {
            m_sendnextpage = value;
            if (value)
            {
                m_ScrollView.ClearMatchInfo(m_ScrollView.CurrentPage + 1);
                NetworkMessageManager.Instance.SendMessage(Network_ID.ListMatch, m_ScrollView.CurrentPage + 1, "");
            }
        }
        get
        {
            return m_sendnextpage;
        }
    }

    private int CurrentPage
    {
        set
        {
            PageLabel.text = string.Format("{0}", value + 1);
            m_ScrollView.CurrentPage = value;
            SetPageButton();
        }
        get
        {
            return m_ScrollView.CurrentPage;
        }
    }

    private void SetPageButton(bool nonecondition_nextbutton = false)
    {
        BackPageButton.isEnabled = CurrentPage != 0 ? true : false;
        bool nextbuttonview = (m_ScrollView.MatchInfoList.Count != 0 && (m_ScrollView.MatchInfoList.Count >= (CurrentPage + 1) * 10)) && nonecondition_nextbutton == false;
        NextPageButton.isEnabled = nextbuttonview;
    }

    void Start ()
    {
        m_ScrollView.Initialize();
        m_ScrollView.m_MessageTargetObject = MyObject;
        NetworkMessageManager.Instance.AddReceivePacketHandler(Network_ID.ListMatch, ReceivePacket_ListMatch);
        NetworkMessageManager.Instance.AddReceivePacketHandler(Network_ID.JoinRoom, ReceivePacket_JoinRoom);

        m_ScrollView.MatchInfoList.Clear();
        m_ScrollView.CurrentPage = 0;

        if (MyNetworkManager.Instance.matchMaker == null)
            MyNetworkManager.Instance.StartMatchMaker();
        NetworkMessageManager.Instance.SendMessage(Network_ID.ListMatch, m_ScrollView.CurrentPage, "");
    }

    public override void StartGameSequence(int option)
    {

    }

    public override int EndGameSequence(GameUISequence togameseq)
    {
        m_ScrollView.MatchInfoList.Clear();
        return 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        NetworkMessageManager.Instance.RemoveReceivePacketHandler(Network_ID.ListMatch, ReceivePacket_ListMatch);
        NetworkMessageManager.Instance.RemoveReceivePacketHandler(Network_ID.JoinRoom, ReceivePacket_JoinRoom);
    }

    private void ReceivePacket_ListMatch(bool success)
    {
        if (success == false)
        {
            SetPageButton();
            return;
        }

        NetworkMessage_ListMatch listmatch = NetworkMessageManager.Instance.GetMessageBase<NetworkMessage_ListMatch>(Network_ID.ListMatch);
        m_ScrollView.AddMatchInfo(listmatch.RoomList);
        if (listmatch.RoomList.Count == 0)
        {
            SendNextPage = false;
            SetPageButton(true);
            return;
        }

        if (SendNextPage)
        {
            SendNextPage = false;
            CurrentPage++;
        }
        else
            SetPageButton();
    }

    private void ReceivePacket_JoinRoom(bool success)
    {
        if (success)
        {
            SetUICameraEnable(true);
            return;
        }

        NetworkMessage_JoinRoom joinroom = NetworkMessageManager.Instance.GetMessageBase<NetworkMessage_JoinRoom>(Network_ID.JoinRoom);
        if (joinroom == null)
            return;

        m_ScrollView.RemoveMatchinfo(joinroom.SendNetworkID);
    }

    private void AddListMatch()
    {
        NetworkMessage_ListMatch listmatch = NetworkMessageManager.Instance.GetMessageBase<NetworkMessage_ListMatch>(Network_ID.ListMatch);
        m_ScrollView.MatchInfoList.AddRange(listmatch.RoomList);
    }

    public void OnClick_BackPage()
    {
        CurrentPage -= 1;
    }

    public void OnClick_NextPage()
    {
        SendNextPage = true;
    }

    public void OnClick_Refresh()
    {
        m_ScrollView.ClearMatchInfo(m_ScrollView.CurrentPage);
        NetworkMessageManager.Instance.SendMessage(Network_ID.ListMatch, m_ScrollView.CurrentPage, "");
    }

    public void OnClick_Fine()
    {
        m_ScrollView.ClearMatchInfo();
        FindNameInput.ReturnStartValue();
        NetworkMessageManager.Instance.SendMessage(Network_ID.ListMatch, m_ScrollView.CurrentPage, FindNameInput.value);
    }
}
