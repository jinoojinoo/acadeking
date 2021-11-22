using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking.Match;

public class UI_RoomList_ScrollView : CustomInfiniteScrollViewMono<UI_RoomList_ScrollViewItem>
{
    private const int MAX_VIEW = 10;

    private List<MatchInfoSnapshot> m_matchinfoList = new List<MatchInfoSnapshot>();
    public List<MatchInfoSnapshot> MatchInfoList
    {
        get
        {
            return m_matchinfoList;
        }
    }

    public void ClearMatchInfo(int page = -1)
    {
        if (page != -1)
        {
            if (MatchInfoList.Count > (page) * MAX_VIEW)
            {
                int startindex = (page) * MAX_VIEW;
                int count = MatchInfoList.Count - startindex;
                MatchInfoList.RemoveRange(startindex, count);
            }
        }
        else
            MatchInfoList.Clear();
    }

    public void RemoveMatchinfo(long netid)
    {
        MatchInfoList.RemoveAll(x => (long)x.networkId == netid);
        InitData();
    }

    public void AddMatchInfo(List<MatchInfoSnapshot> list)
    {
        ResetPosition();
        MatchInfoList.AddRange(list);
        InitData();
    }

    private int m_currentPage;
    public int CurrentPage
    {
        set
        {
            m_currentPage = value;
            InitData();
        }
        get
        {
            return m_currentPage;
        }
    }

    protected override void InitData()
    {
        RemoveBadConnent();
        ScrollView.DataNum = MatchInfoList.Count > MAX_VIEW ? MAX_VIEW : MatchInfoList.Count;
        ScrollView.Init();

        m_UIScrollView.enabled = ScrollView.DataNum <= 4 ? false : true;
    }

    private void RemoveBadConnent()
    {
        NetworkMessage_JoinRoom joinroom = NetworkMessageManager.Instance.GetMessageBase<NetworkMessage_JoinRoom>(Network_ID.JoinRoom);
        if (joinroom == null)
            MatchInfoList.RemoveAll(x => joinroom.BadNetworkID.Contains((long)x.networkId));
    }

    protected override UI_RoomList_ScrollViewItem InitObject(GameObject obj, int dataPos)
    {
        UI_RoomList_ScrollViewItem item = base.InitObject(obj, dataPos) as UI_RoomList_ScrollViewItem;
        if (item == null)
        {
            obj.SetActive(false);
            return null;
        }

        int matchindex = m_currentPage * MAX_VIEW + dataPos;
        if (MatchInfoList.Count <= matchindex)
        {
            obj.SetActive(false);
            return null;
        }

        item.Init(MatchInfoList[matchindex]);
        obj.SetActive(true);

        return item;
    }
}
