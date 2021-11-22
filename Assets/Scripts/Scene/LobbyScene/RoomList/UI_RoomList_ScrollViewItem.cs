using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking.Match;

public class UI_RoomList_ScrollViewItem : InfiniteObjectInfo
{
    public UILabel TitleLabel;
    public UILabel RoundLabel;
    public UILabel TimeLabel;

    private MatchInfoSnapshot m_matchinfo = null;
    public void Init(MatchInfoSnapshot matchinfo)
    {
        string[] infos = matchinfo.name.Split(',');
        TitleLabel.text = infos[0];
        RoundLabel.text = string.Format("Round {0:0}", int.Parse(infos[1]) + 1);
        TimeLabel.text = string.Format("{0:00} Sec", GlobalValue_Table.Instance.GetRoundTime(int.Parse(infos[2])));

        m_matchinfo = matchinfo;
    }

    private void OnClick()
    {
        MyNetworkManager.Instance.CreateRoomInfo.Init(m_matchinfo.name);
        NetworkMessageManager.Instance.SendMessage(Network_ID.JoinRoom, (long)m_matchinfo.networkId);
    }
}
